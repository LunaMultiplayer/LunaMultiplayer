using LiteDB;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Enums;
using Server.Events;
using Server.Server;
using Server.Settings.Structures;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.VesselRelay
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces
    /// We use a database file that uses memory instead of RAM but it's slower
    /// </summary>
    public static class VesselRelaySystemDataBase
    {
        #region VesselRelayDB Structure

        private class VesselRelayDbItem : VesselRelayItem
        {
            public ObjectId Id { get; } = ObjectId.NewObjectId();

            public VesselRelayDbItem()
            {
            }

            public VesselRelayDbItem(int subspaceId, Guid vesselId, double gameTime, VesselBaseMsgData msg) : base(subspaceId, vesselId, gameTime, msg) { }
        }

        #endregion

        private static readonly string DbFile = Path.Combine(ServerContext.UniverseDirectory, "Relay", "RelayDatabase.db");
        private static readonly LiteDatabase DataBase = new LiteDatabase(DbFile);
        private static LiteCollection<VesselRelayDbItem> DbCollection => DataBase.GetCollection<VesselRelayDbItem>();

        static VesselRelaySystemDataBase() => ExitEvent.ServerClosing += Destructor;

        private static void Destructor()
        {
            DataBase.Dispose();
            FileHandler.FileDelete(DbFile);
        }

        /// <summary>
        /// This method relays a message to the other clients in the same subspace.
        /// In case there are other players in OLDER subspaces it stores it in their queue for further processing
        /// </summary>
        public static void HandleVesselMessage(ClientStructure client, VesselBaseMsgData msg)
        {
            if (client.Subspace == -1) return;

            var vesselId = msg.VesselId;
            var gameTime = msg.GameTime;

            MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg);

            if (GeneralSettings.SettingsStore.ShowVesselsInThePast)
            {
                foreach (var subspace in WarpSystem.GetFutureSubspaces(client.Subspace))
                    MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg, subspace);
            }

            if (!VesselRelaySystem.ShouldStoreMessage(vesselId, msg.VesselMessageType)) return;

            //The client is running in the future so here we adjust the real sent time of the message
            msg.SentTime += WarpSystem.GetSubspaceTimeDifference(client.Subspace);
            foreach (var subspace in WarpSystem.GetPastSubspaces(client.Subspace))
            {
                //This is the case when a user reverted (the message received has a game time LOWER than the last message received). 
                //To handle this, we remove all the messages that we received UNTIL this revert.
                if (DbCollection.Exists(x => x.VesselId == vesselId && x.GameTime > gameTime))
                {
                    DbCollection.Delete(x => x.VesselId == vesselId && x.GameTime > gameTime);
                }

                DbCollection.Insert(new VesselRelayDbItem(subspace, vesselId, gameTime, msg));
            }

            DbCollection.EnsureIndex(x => x.Id);
            DbCollection.EnsureIndex(x => x.SubspaceId);
            DbCollection.EnsureIndex(x => x.VesselId);
            DbCollection.EnsureIndex(x => x.GameTime);
        }

        /// <summary>
        /// Creates a new subspace and sets its message queue from a past subspace.
        /// Must be called AFTER the subspace is created in the warp context.
        /// </summary>
        public static void CreateNewSubspace(int subspaceId)
        {
            //If the new subspace is the most advanced in time skip all this method
            if (!WarpSystem.GetFutureSubspaces(subspaceId).Any()) return;

            var pastSubspaces = WarpSystem.GetPastSubspaces(subspaceId);
            if (pastSubspaces.Any())
            {
                //Here we get the PAST subspace that is closest in time to the one we got as parameter
                var closestPastSubspace = WarpContext.Subspaces
                    .Where(s => pastSubspaces.Contains(s.Key))
                    .OrderByDescending(s => s.Value)
                    .Select(s => s.Key)
                    .First();

                //Now we are going to get all the messages from that PAST subspace and remove the ones that are too old for our new subspace.
                //Te idea is that we are going to use a queue with a lot of messages that came from future and use for our new subspace
                //But in that queue there might be messages that are too old so we can remove them to save space
                var subspaceTime = WarpSystem.GetSubspaceTime(subspaceId);
                var messageQueue = DbCollection.Find(v => v.SubspaceId == closestPastSubspace && v.Msg.SentTime < subspaceTime).ToList();

                //Once we've got the queue clean of old messages we add it do the db 
                //so the future subspaces fill up our queue.
                messageQueue.ForEach(m => m.SubspaceId = subspaceId);
                DbCollection.InsertBulk(messageQueue);
                DbCollection.EnsureIndex(x => x.Id);
                DbCollection.EnsureIndex(x => x.SubspaceId);
                DbCollection.EnsureIndex(x => x.GameTime);
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            DbCollection.Delete(m => m.SubspaceId == subspaceId);
        }

        /// <summary>
        /// This method should be called in a thread. 
        /// It runs over the old messages and sends them once the subspace time matches the message send time.
        /// </summary>
        public static async void RelayOldVesselMessages()
        {
            while (ServerContext.ServerRunning)
            {
                var messagesByGroup = DbCollection.FindAll().GroupBy(m => m.SubspaceId);
                foreach (var subspace in messagesByGroup)
                {
                    var subspaceTime = WarpSystem.GetSubspaceTime(subspace.Key);
                    var msgToSend = subspace.Where(m => subspaceTime >= m.Msg.SentTime).ToList();
                    msgToSend.ForEach(m =>
                    {
                        MessageQueuer.SendMessageToSubspace<VesselSrvMsg>(m.Msg, m.SubspaceId);
                        DbCollection.Delete(m.Id);
                    });
                }

                await Task.Delay(IntervalSettings.SettingsStore.SendReceiveThreadTickMs);
            }
        }

        /// <summary>
        /// Shrink the database to reduce the file size
        /// </summary>
        public static void ShrinkDatabase()
        {
            switch (RelaySettings.SettingsStore.RelaySystemMode)
            {
                case RelaySystemMode.Database:
                    DataBase.Shrink();
                    break;
            }
        }
    }
}

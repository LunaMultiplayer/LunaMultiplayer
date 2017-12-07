using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LiteDB;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings;

namespace Server.System
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces
    /// We use a database file that uses memory instead of RAM but it's slower
    /// </summary>
    public static class VesselRelaySystemDataBase
    {
        #region Destructor

        private sealed class Destructor
        {
            ~Destructor()
            {
                DataBase.Dispose();
            }
        }

        private static readonly Destructor Finalise = new Destructor();

        #endregion

        #region VesselRelayMsg Structure

        private class VesselRelayMessage
        {
            public ObjectId Id = ObjectId.NewObjectId();

            public int SubspaceId { get; set; }
            public VesselBaseMsgData Msg { get; }

            public VesselRelayMessage(int subspaceId, VesselBaseMsgData msg)
            {
                SubspaceId = subspaceId;
                Msg = msg;
            }
        }

        #endregion

        private static readonly string DbFile = Path.Combine(ServerContext.UniverseDirectory, "Relay", "RelayDatabase.db");
        private static readonly LiteDatabase DataBase = new LiteDatabase(DbFile);
        private static LiteCollection<VesselRelayMessage> DbCollection => DataBase.GetCollection<VesselRelayMessage>();

        /// <summary>
        /// This method relays a message to the other clients in the same subspace.
        /// In case there are other players in OLDER subspaces it stores it in their queue for further processing
        /// </summary>
        public static void HandleVesselMessage(ClientStructure client, VesselBaseMsgData msg)
        {
            if (client.Subspace == -1) return;

            MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg);

            if (GeneralSettings.SettingsStore.ShowVesselsInThePast)
            {
                foreach (var subspace in WarpSystem.GetFutureSubspaces(client.Subspace))
                    MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg, subspace);
            }

            //The client is running in the future so here we adjust the real sent time of the message
            msg.SentTime += WarpSystem.GetSubspaceTimeDifference(client.Subspace);
            foreach (var subspace in WarpSystem.GetPastSubspaces(client.Subspace))
            {
                DbCollection.Insert(new VesselRelayMessage(subspace, msg));
            }

            DbCollection.EnsureIndex(x => x.Id);
            DbCollection.EnsureIndex(x => x.SubspaceId);
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
                var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspaceId);
                var messageQueue = DbCollection.Find(v=> v.SubspaceId == closestPastSubspace && v.Msg.SentTime < subspaceTime).ToList();

                //Once we've got the queue clean of old messages we add it do the db 
                //so the future subspaces fill up our queue.
                messageQueue.ForEach(m => m.SubspaceId = subspaceId);
                DbCollection.InsertBulk(messageQueue);
                DbCollection.EnsureIndex(x => x.Id);
                DbCollection.EnsureIndex(x => x.SubspaceId);
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            DbCollection.Delete(m => m.SubspaceId == subspaceId);
            DataBase.Shrink();
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
                    var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspace.Key);
                    var msgToSend = subspace.Where(m => subspaceTime >= m.Msg.SentTime).ToList();
                    msgToSend.ForEach(m=>
                    {
                        MessageQueuer.SendMessageToSubspace<VesselSrvMsg>(m.Msg, m.SubspaceId);
                        DbCollection.Delete(m.Id);
                    });
                }

                DataBase.Shrink();
                await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
            }
        }
    }
}

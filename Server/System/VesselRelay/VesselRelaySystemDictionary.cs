using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings;
using Server.Utilities;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System.VesselRelay
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces. 
    /// We use a dictionary that uses RAM but it's faster
    /// </summary>
    public static class VesselRelaySystemDictionary
    {
        private static readonly ConcurrentDictionary<int, ConcurrentDictionary<Guid, ConcurrentQueue<VesselRelayItem>>> OldSubspaceVesselMessages =
            new ConcurrentDictionary<int, ConcurrentDictionary<Guid, ConcurrentQueue<VesselRelayItem>>>();

        /// <summary>
        /// This method relays a message to the other clients in the same subspace.
        /// In case there are other players in OLDER subspaces it stores it in their queue for further processing
        /// </summary>
        public static void HandleVesselMessage(ClientStructure client, dynamic msg)
        {
            if (client.Subspace == -1) return;

            var vesselId = (Guid)msg.VesselId;
            var gameTime = (double)msg.GameTime;

            MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg);

            if (GeneralSettings.SettingsStore.ShowVesselsInThePast)
            {
                //Here we send this update to all the players in the FUTURE
                foreach (var subspace in WarpSystem.GetFutureSubspaces(client.Subspace))
                    MessageQueuer.RelayMessageToSubspace<VesselSrvMsg>(client, msg, subspace);
            }

            //In case the client is running in the future here we adjust the real sent time of the message
            msg.SentTime += WarpSystem.GetSubspaceTimeDifference(client.Subspace);
            foreach (var subspace in WarpSystem.GetPastSubspaces(client.Subspace))
            {
                var vesselsAndQueues = OldSubspaceVesselMessages.GetOrAdd(subspace, new ConcurrentDictionary<Guid, ConcurrentQueue<VesselRelayItem>>());
                var vesselQueue = vesselsAndQueues.GetOrAdd(vesselId, new ConcurrentQueue<VesselRelayItem>());

                //This is the case when a user reverted (the message received has a game time LOWER than the last message received). 
                //To handle this, we remove all the messages that we received UNTIL this revert.
                if (vesselQueue.Last().GameTime > gameTime)
                {
                    while (vesselQueue.TryPeek(out var peekValue) && peekValue.GameTime > gameTime)
                    {
                        vesselQueue.TryDequeue(out _);
                    }
                }

                vesselQueue.Enqueue(new VesselRelayItem(msg.SubspaceId, vesselId, gameTime, msg));
            }
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
                //Here we get the PAST subspace that is closest in time to the one we've got as a parameter
                var closestPastSubspace = WarpContext.Subspaces
                    .Where(s => pastSubspaces.Contains(s.Key))
                    .OrderByDescending(s => s.Value)
                    .Select(s => s.Key)
                    .First();

                //Now we clone it's message queue
                if (OldSubspaceVesselMessages.TryGetValue(closestPastSubspace, out var vesselsAndQueues))
                {
                    foreach (var vesselQueue in vesselsAndQueues)
                    {
                        var messageQueue = vesselQueue.Value.CloneConcurrentQueue();

                        //As we cloned a queue from a PAST subspace, we may have many messages 
                        //that are TOO OLD as we are in a future subspace. Therefore, we remove the old
                        //messages for this subspace
                        var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspaceId);

                        while (messageQueue.TryPeek(out var relayItem))
                        {
                            if (relayItem.Msg.SentTime >= subspaceTime)
                                break;
                        }

                        //Once we've got the queue clean of old messages we add it do the dictionary 
                        //so the future subspaces fill up our queue.
                        OldSubspaceVesselMessages.TryAdd(subspaceId, new ConcurrentDictionary<Guid, ConcurrentQueue<VesselRelayItem>>
                        {
                            [vesselQueue.Key] = messageQueue
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            OldSubspaceVesselMessages.TryRemove(subspaceId, out var _);
        }

        /// <summary>
        /// This method should be called in a thread. 
        /// It runs over the old messages and sends them once the subspace time matches the message send time.
        /// </summary>
        public static async void RelayOldVesselMessages()
        {
            while (ServerContext.ServerRunning)
            {
                foreach (var subspaceVessels in OldSubspaceVesselMessages.Where(m => !m.Value.IsEmpty))
                {
                    var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspaceVessels.Key);

                    foreach (var queue in subspaceVessels.Value)
                    {
                        while (queue.Value.TryPeek(out var relayItem) && subspaceTime >= relayItem.Msg.SentTime)
                        {
                            queue.Value.TryDequeue(out relayItem);
                            MessageQueuer.SendMessageToSubspace<VesselSrvMsg>(relayItem.Msg, subspaceVessels.Key);
                        }
                    }
                }

                await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
            }
        }
    }
}

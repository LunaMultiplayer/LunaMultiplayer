using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Server;
using Server.Settings;
using Server.Utilities;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Server.System
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces. 
    /// We use a dictionary that uses RAM but it's faster
    /// </summary>
    public static class VesselRelaySystemDictionary
    {
        private static readonly ConcurrentDictionary<int, ConcurrentQueue<VesselBaseMsgData>> OldVesselMessages =
            new ConcurrentDictionary<int, ConcurrentQueue<VesselBaseMsgData>>();

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
                OldVesselMessages.GetOrAdd(subspace, new ConcurrentQueue<VesselBaseMsgData>()).Enqueue(msg);
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
                //Here we get the PAST subspace that is closest in time to the one we got as parameter
                var closestPastSubspace = WarpContext.Subspaces
                    .Where(s => pastSubspaces.Contains(s.Key))
                    .OrderByDescending(s => s.Value)
                    .Select(s => s.Key)
                    .First();
                
                //Now we clone it's message queue
                if (OldVesselMessages.TryGetValue(closestPastSubspace, out var concurrentQueue))
                {
                    var messageQueue = concurrentQueue.CloneConcurrentQueue();

                    //As we cloned a queue from a PAST subspace, we may have many messages 
                    //that are TOO OLD as we are in a future subspace. Therefore, we remove the old
                    //messages for this subspace
                    var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspaceId);
                    while (messageQueue.TryDequeue(out var msg))
                    {
                        if (msg.SentTime >= subspaceTime)
                            break;
                    }

                    //Once we've got the queue clean of old messages we add it do the dictionary 
                    //so the future subspaces fill up our queue.
                    OldVesselMessages.TryAdd(subspaceId, messageQueue);
                }
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            OldVesselMessages.TryRemove(subspaceId, out var _);
        }

        /// <summary>
        /// This method should be called in a thread. 
        /// It runs over the old messages and sends them once the subspace time matches the message send time.
        /// </summary>
        public static async void RelayOldVesselMessages()
        {
            while (ServerContext.ServerRunning)
            {
                foreach (var keyVal in OldVesselMessages.Where(m => !m.Value.IsEmpty))
                {
                    var subspaceTime = WarpSystem.GetCurrentSubspaceTime(keyVal.Key);

                    while (keyVal.Value.TryPeek(out var msg) && subspaceTime >= msg.SentTime)
                    {
                        keyVal.Value.TryDequeue(out msg);
                        MessageQueuer.SendMessageToSubspace<VesselSrvMsg>(msg, keyVal.Key);
                    }
                }

                await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
            }
        }
    }
}

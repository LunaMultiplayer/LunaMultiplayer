using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Server;
using LunaServer.Settings;

namespace LunaServer.System
{
    /// <summary>
    /// This class relay the vessel messages to the correct subspaces
    /// </summary>
    public class VesselRelaySystem
    {
        private static readonly ConcurrentDictionary<int, ConcurrentQueue<VesselBaseMsgData>> OldVesselMessages = 
            new ConcurrentDictionary<int, ConcurrentQueue<VesselBaseMsgData>>();

        /// <summary>
        /// This method relays a message to the other clients in the same subspace and in case there are other players 
        /// in older subspaces it stores it for further processing
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
                if (!OldVesselMessages.ContainsKey(client.Subspace))
                {
                    OldVesselMessages.TryAdd(subspace, new ConcurrentQueue<VesselBaseMsgData>());
                }

                OldVesselMessages[subspace].Enqueue(msg);
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

            var subspaceTime = WarpSystem.GetCurrentSubspaceTime(subspaceId);

            //Here we get the PAST subspace that is closest in time to the one we got as parameter
            var pastSubspaces = WarpSystem.GetPastSubspaces(subspaceId);

            if (pastSubspaces.Any())
            {
                var closestPastSubspace = WarpContext.Subspaces
                    .Where(s => pastSubspaces.Contains(s.Key))
                    .OrderByDescending(s => s.Value)
                    .Select(s => s.Key)
                    .First();

                var originalqueue = OldVesselMessages[closestPastSubspace];

                var messages = new VesselBaseMsgData[originalqueue.Count];
                originalqueue.CopyTo(messages, 0);

                var messageQueue = new ConcurrentQueue<VesselBaseMsgData>(messages);

                //Now we remove the messages that are too old for this subspace
                VesselBaseMsgData msg;
                while (messageQueue.TryDequeue(out msg))
                {
                    if (msg.SentTime >= subspaceTime)
                        break;
                }

                OldVesselMessages.TryAdd(subspaceId, messageQueue);
            }
        }

        /// <summary>
        /// Removes a subspace
        /// </summary>
        public static void RemoveSubspace(int subspaceId)
        {
            ConcurrentQueue<VesselBaseMsgData> messageQueue;
            OldVesselMessages.TryRemove(subspaceId, out messageQueue);
        }

        /// <summary>
        /// This method should be called in a thread. 
        /// It runs over the old messages and sends them once the subspace time matches the message send time.
        /// </summary>
        public static void RelayOldVesselMessages()
        {
            while (ServerContext.ServerRunning)
            {
                foreach (var keyVal in OldVesselMessages.Where(m => !m.Value.IsEmpty))
                {
                    var subspaceTime = WarpSystem.GetCurrentSubspaceTime(keyVal.Key);

                    VesselBaseMsgData msg;
                    while(keyVal.Value.TryPeek(out msg) && subspaceTime >= msg.SentTime)
                    {
                        keyVal.Value.TryDequeue(out msg);
                        MessageQueuer.SendMessageToSubspace<VesselSrvMsg>(msg, keyVal.Key);
                    }
                }

                Thread.Sleep(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
            }
        }
    }
}

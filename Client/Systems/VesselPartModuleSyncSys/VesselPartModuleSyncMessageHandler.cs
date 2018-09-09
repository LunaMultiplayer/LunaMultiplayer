using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPartModuleSyncSys
{
    public class VesselPartModuleSyncMessageHandler : SubSystem<VesselPartModuleSyncSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPartSyncMsgData msgData) || !System.PartSyncSystemReady) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            if (!System.VesselPartsSyncs.ContainsKey(msgData.VesselId))
            {
                System.VesselPartsSyncs.TryAdd(msgData.VesselId, new VesselPartSyncQueue());
            }

            if (System.VesselPartsSyncs.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var resource) && resource.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear his message queue and start from scratch
                    queue.Clear();
                }

                queue.Enqueue(msgData);
            }
        }
    }
}

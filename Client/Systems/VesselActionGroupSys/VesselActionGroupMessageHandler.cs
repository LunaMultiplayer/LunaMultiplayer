using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselActionGroupSys
{
    public class VesselActionGroupMessageHandler : SubSystem<VesselActionGroupSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselActionGroupMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;
            
            if (!System.VesselActionGroups.ContainsKey(msgData.VesselId))
            {
                System.VesselActionGroups.TryAdd(msgData.VesselId, new VesselActionGroupQueue());
            }

            if (System.VesselActionGroups.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var update) && update.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear his message queue and start from scratch
                    queue.Clear();
                }

                queue.Enqueue(msgData);
            }
        }
    }
}

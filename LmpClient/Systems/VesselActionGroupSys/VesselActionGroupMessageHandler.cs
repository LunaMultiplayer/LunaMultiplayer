using System.Collections.Concurrent;
using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselActionGroupSys
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

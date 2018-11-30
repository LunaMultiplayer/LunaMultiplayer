using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselUndockSys
{
    public class VesselUndockMessageHandler : SubSystem<VesselUndockSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselUndockMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            if (!System.VesselUndocks.ContainsKey(msgData.VesselId))
            {
                System.VesselUndocks.TryAdd(msgData.VesselId, new VesselUndockQueue());
            }

            if (System.VesselUndocks.TryGetValue(msgData.VesselId, out var queue))
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

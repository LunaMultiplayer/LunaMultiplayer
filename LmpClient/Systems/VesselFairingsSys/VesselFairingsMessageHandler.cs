using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselFairingsSys
{
    public class VesselFairingsMessageHandler : SubSystem<VesselFairingsSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselFairingMsgData msgData)) return;

            //We received a msg for our own controlled/updated vessel so ignore it
            if (!VesselCommon.DoVesselChecks(msgData.VesselId))
                return;

            if (!System.VesselFairings.ContainsKey(msgData.VesselId))
            {
                System.VesselFairings.TryAdd(msgData.VesselId, new VesselFairingQueue());
            }

            if (System.VesselFairings.TryGetValue(msgData.VesselId, out var queue))
            {
                if (queue.TryPeek(out var value) && value.GameTime > msgData.GameTime)
                {
                    //A user reverted, so clear his message queue and start from scratch
                    queue.Clear();
                }

                queue.Enqueue(msgData);
            }
        }
    }
}

using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.VesselUtilities;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LmpClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return;

            var vesselId = msgData.VesselId;
            if (!VesselCommon.DoVesselChecks(vesselId))
                return;
            
            if (VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var currentUpdate) && currentUpdate.GameTimeStamp > msgData.GameTime)
            {
                //A user reverted, so clear it and start from scratch
                System.RemoveVessel(vesselId);
            }

            if (!VesselPositionSystem.CurrentVesselUpdate.ContainsKey(vesselId))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, new VesselPositionUpdate(msgData));
                VesselPositionSystem.TargetVesselUpdateQueue.TryAdd(vesselId, new PositionUpdateQueue());
            }
            else
            {
                VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(vesselId, out var queue);
                queue?.Enqueue(msgData);
            }
        }
    }
}

using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionAltSys
{
    public class VesselPositionMessageAltHandler : SubSystem<VesselPositionAltSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselPositionMsgData;
            if (msgData == null) return;

            var update = new VesselPositionAltUpdate(msgData);

            var vesselId = update.VesselId;

            if (!VesselPositionAltSystem.CurrentVesselUpdate.TryGetValue(update.VesselId, out var existingPositionUpdate))
            {
                VesselPositionAltSystem.CurrentVesselUpdate.TryAdd(vesselId, update);
                VesselPositionAltSystem.TargetVesselUpdate.TryAdd(vesselId, update);
            }
            else
            {
                if (existingPositionUpdate.SentTime < update.SentTime && existingPositionUpdate.InterpolationFinished)
                {
                    update.Vessel = VesselPositionAltSystem.CurrentVesselUpdate[vesselId].Vessel;
                    update.Body = VesselPositionAltSystem.CurrentVesselUpdate[vesselId].Body;

                    VesselPositionAltSystem.CurrentVesselUpdate[vesselId] = VesselPositionAltSystem.TargetVesselUpdate[vesselId];
                    VesselPositionAltSystem.TargetVesselUpdate[vesselId] = update;
                }
            }
        }
    }
}

using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselRemoveSys;
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
            if (!(messageData is VesselPositionMsgData msgData)) return;

            var update = new VesselPositionAltUpdate(msgData);

            var vesselId = update.VesselId;

            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselId))
                return;

            if (!VesselPositionAltSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionAltSystem.CurrentVesselUpdate.TryAdd(vesselId, update);
                VesselPositionAltSystem.TargetVesselUpdate.TryAdd(vesselId, update);
            }
            else
            {
                if (existingPositionUpdate.SentTime < update.SentTime &&
                    (existingPositionUpdate.InterpolationFinished || !existingPositionUpdate.InterpolationStarted))
                {
                    update.Body = existingPositionUpdate.Body;

                    if (VesselPositionAltSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                    {
                        VesselPositionAltSystem.CurrentVesselUpdate.AddOrUpdate(vesselId, existingTargetPositionUpdate,
                            (key, existingVal) => existingTargetPositionUpdate);

                        VesselPositionAltSystem.TargetVesselUpdate.AddOrUpdate(vesselId, update,
                            (key, existingVal) => update);
                    }
                }
            }
        }
    }
}

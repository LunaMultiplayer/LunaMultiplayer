using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselPositionMsgData msgData)) return;

            var update = new VesselPositionUpdate(msgData);

            var vesselId = update.VesselId;

            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselId))
                return;

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, update);
                VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, update);
            }
            else
            {
                if (existingPositionUpdate.SentTime < update.SentTime &&
                    (existingPositionUpdate.InterpolationFinished || !existingPositionUpdate.InterpolationStarted))
                {
                    update.Body = existingPositionUpdate.Body;

                    if (VesselPositionSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                    {
                        VesselPositionSystem.CurrentVesselUpdate.AddOrUpdate(vesselId, existingTargetPositionUpdate,
                            (key, existingVal) => existingTargetPositionUpdate);

                        VesselPositionSystem.TargetVesselUpdate.AddOrUpdate(vesselId, update,
                            (key, existingVal) => update);
                    }
                }
            }
        }
    }
}

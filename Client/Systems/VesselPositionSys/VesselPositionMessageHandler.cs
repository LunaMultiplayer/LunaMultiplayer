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
            
            var vesselId = msgData.VesselId;

            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselId))
                return;

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, new VesselPositionUpdate(msgData));
                VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, new VesselPositionUpdate(msgData));
            }
            else
            {
                if (existingPositionUpdate.MsgData.SentTime < msgData.SentTime &&
                    (existingPositionUpdate.InterpolationFinished || !existingPositionUpdate.InterpolationStarted))
                {
                    if (VesselPositionSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                    {
                        existingPositionUpdate.MsgData = existingTargetPositionUpdate.MsgData;

                        var newUpdate = new VesselPositionUpdate(msgData);
                        VesselPositionSystem.TargetVesselUpdate.AddOrUpdate(vesselId, newUpdate, (key, existingVal) => newUpdate);
                    }
                }
            }
        }
    }
}

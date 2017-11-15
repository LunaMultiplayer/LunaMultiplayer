using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageHandler : SubSystem<VesselPositionSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return;
            
            var vesselId = msgData.VesselId;

            //Ignore vessel updates for our own controlled vessel
            if (LockSystem.LockQuery.ControlLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return;

            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselId))
                return;

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
                VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
            }
            else
            {
                if (existingPositionUpdate.SentTime < msgData.SentTime &&
                    (existingPositionUpdate.InterpolationFinished || !existingPositionUpdate.InterpolationStarted))
                {
                    if (VesselPositionSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                    {
                        existingPositionUpdate.ResetFields();
                        existingTargetPositionUpdate.ResetFields();
                        MessageToPositionTransfer.UpdateFromUpdate(existingTargetPositionUpdate, existingPositionUpdate);
                        MessageToPositionTransfer.UpdateFromMessage(msg, existingTargetPositionUpdate);
                    }
                }
            }
        }
    }
}

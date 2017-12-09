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

            //Ignore updates if vessel is in kill list
            if (SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(vesselId))
                return;

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));

                if (SettingsSystem.CurrentSettings.InterpolationEnabled)
                    VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
            }
            else
            {
                //Here we check that the message timestamp is lower than the message we received. UDP is not reliable and can deliver packets not in order!
                if (existingPositionUpdate.TimeStamp < msgData.TimeStamp)
                {
                    if (SettingsSystem.CurrentSettings.InterpolationEnabled)
                    {
                        var existingTargetPositionUpdate = VesselPositionSystem.TargetVesselUpdate.GetOrAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
                        if (existingTargetPositionUpdate.TimeStamp < msgData.TimeStamp)
                        {
                            if (existingPositionUpdate.InterpolationFinished || !existingPositionUpdate.InterpolationStarted)
                            {
                                existingPositionUpdate.Restart();
                            }

                            MessageToPositionTransfer.UpdateFromMessage(msg, existingTargetPositionUpdate);
                        }
                    }
                    else
                    {
                        existingPositionUpdate.ResetFields();
                        MessageToPositionTransfer.UpdateFromMessage(msg, existingPositionUpdate);
                    }
                }
            }
        }
    }
}

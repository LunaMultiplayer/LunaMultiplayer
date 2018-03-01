using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
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

            //Ignore vessel updates for our own updated vessels
            if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return;

            //Ignore vessel updates for our own updated vessels
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return;

            //Ignore updates if vessel is in kill list
            if (VesselRemoveSystem.Singleton.VesselWillBeKilled(vesselId))
                return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoPosition(msgData);

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
                VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, MessageToPositionTransfer.CreateFromMessage(msg));
            }
            else
            {
                if (VesselPositionSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                {
                    if (SettingsSystem.CurrentSettings.InterpolationEnabled)
                    {
                        //Here we set the start position to the current VESSEL position in order to LERP correctly
                        existingPositionUpdate.Restart();
                    }
                    else
                    {
                        //Here we just set the interpolation as not started
                        existingPositionUpdate.ResetFields();
                    }

                    //Overwrite the TARGET data with the data we've received in the message
                    MessageToPositionTransfer.UpdateFromMessage(msg, existingTargetPositionUpdate);
                }
            }
        }
    }
}

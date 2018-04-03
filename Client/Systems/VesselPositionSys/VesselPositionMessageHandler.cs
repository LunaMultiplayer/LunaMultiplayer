using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
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

            if (!VesselCommon.DoVesselChecks(vesselId))
                return;

            //Vessel might exist in the store but not in game (if the vessel is in safety bubble for example)
            VesselsProtoStore.UpdateVesselProtoPosition(msgData);

            if (!VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var existingPositionUpdate))
            {
                var current = MessageToPositionTransfer.CreateFromMessage(msg);
                var target = MessageToPositionTransfer.CreateFromMessage(msg);

                VesselPositionSystem.CurrentVesselUpdate.TryAdd(vesselId, current);
                VesselPositionSystem.TargetVesselUpdate.TryAdd(vesselId, target);
                VesselPositionSystem.TargetVesselUpdateQueue.TryAdd(vesselId, new FixedSizedConcurrentQueue<VesselPositionUpdate>(VesselPositionSystem.MaxQueuedUpdates));

                current.SetTarget(target);
            }
            else
            {
                if (SettingsSystem.CurrentSettings.InterpolationEnabled)
                {
                    if (VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(vesselId, out var queue))
                    {
                        queue.Enqueue(MessageToPositionTransfer.CreateFromMessage(msg));
                    }
                }
                else
                {
                    if (VesselPositionSystem.TargetVesselUpdate.TryGetValue(vesselId, out var existingTargetPositionUpdate))
                    {                    
                        //Overwrite the TARGET data with the data we've received in the message
                        MessageToPositionTransfer.UpdateFromMessage(msg, existingTargetPositionUpdate);
                        //Here we just set the interpolation as not started
                        existingPositionUpdate.SetTarget(existingTargetPositionUpdate);
                    }
                }
            }
        }
    }
}

using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.VesselUtilities;

namespace LmpClient.Systems.VesselEvaEditorSys
{
    public class VesselEvaEditorEvents : SubSystem<VesselEvaEditorSystem>
    {
        public void EVAConstructionModePartAttached(Vessel vessel, Part part)
        {
            if (VesselCommon.IsSpectating) return;
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
        }

        public void EVAConstructionModePartDetached(Vessel vessel, Part part)
        {
            if (VesselCommon.IsSpectating) return;
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
        }

        public void VesselCreated(Vessel vessel)
        {
            if (System.DetachingPart)
            {
                LockSystem.Singleton.AcquireUpdateLock(vessel.id, true, true);
                LockSystem.Singleton.AcquireUnloadedUpdateLock(vessel.id, true, true);

                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel);
            }
        }

        public void OnDroppingPart()
        {
            System.DetachingPart = true;
        }

        public void OnDroppedPart()
        {
            System.DetachingPart = false;
        }

        public void OnAttachingPart(Part part)
        {
            if (part.vessel)
                VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(part.vessel);
        }
    }
}

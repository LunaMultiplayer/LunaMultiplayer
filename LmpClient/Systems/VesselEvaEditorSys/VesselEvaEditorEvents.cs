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
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, reason: "EVA construction: part attached");
        }

        public void EVAConstructionModePartDetached(Vessel vessel, Part part)
        {
            if (VesselCommon.IsSpectating) return;
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, reason: "EVA construction: part detached");
        }

        public void VesselCreated(Vessel vessel)
        {
            if (vessel == null || VesselCommon.IsSpectating) return;

            // Two distinct creation paths route through GameEvents.onNewVesselCreated and need to
            // be sent to the server here:
            //   1. EVA Construction Mode part drops - already gated by System.DetachingPart, set by
            //      EVAConstructionEvent.onDroppingPart/onDroppedPart.
            //   2. Breaking Ground deployable science placements - the kerbal places a science
            //      part from inventory and KSP spins it up as a new vessel of type
            //      DeployedSciencePart / DeployedScienceController. This path does NOT fire any
            //      EVAConstructionEvent, so without this branch the vessel was created locally,
            //      had locks acquired by VesselLockSystem's bulk pass, but its proto was never
            //      transmitted - leaving an "orphan" lock on the server and a vessel that only
            //      existed in the placing player's local save.
            var isEvaConstructionDrop = System.DetachingPart;
            var isDeployableScience = vessel.vesselType == VesselType.DeployedSciencePart
                                   || vessel.vesselType == VesselType.DeployedScienceController;

            if (!isEvaConstructionDrop && !isDeployableScience) return;

            LockSystem.Singleton.AcquireUpdateLock(vessel.id, true, true);
            LockSystem.Singleton.AcquireUnloadedUpdateLock(vessel.id, true, true);

            var reason = isEvaConstructionDrop
                ? "EVA construction: new vessel from detached part"
                : "Breaking Ground: deployed science part placed";

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, reason: reason);
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
                VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(part.vessel, true, "EVA construction: attaching part");
        }
    }
}

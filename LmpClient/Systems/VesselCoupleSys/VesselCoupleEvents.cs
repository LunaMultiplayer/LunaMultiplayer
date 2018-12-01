using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using System;
using LmpCommon.Enums;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleEvents : SubSystem<VesselCoupleSystem>
    {
        public void CoupleStart(Part partFrom, Part partTo)
        {
            if (VesselCommon.IsSpectating || System.IgnoreEvents) return;

            //If neither the vessel 1 or vessel2 locks belong to us, ignore the coupling
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(partTo.vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Detected part couple! Part: {partFrom.partName} Vessel: {partFrom.vessel.id} - CoupledPart: {partTo.partName} CoupledVessel: {partTo.vessel.id}");
        }

        public void CoupleComplete(Part partFrom, Part partTo, Guid removedVesselId)
        {
            if (VesselCommon.IsSpectating || System.IgnoreEvents) return;

            //If neither the vessel 1 or vessel2 locks belong to us, ignore the coupling
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(removedVesselId, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Couple complete! Removed vessel: {removedVesselId}");

            //Yes, the couple event is called by the WEAK vessel!!
            var trigger = partTo.FindModuleImplementing<ModuleDockingNode>() != null ? CoupleTrigger.DockingNode :
                partTo.FindModuleImplementing<ModuleGrappleNode>() != null ? CoupleTrigger.GrappleNode :
                partTo.FindModuleImplementing<KerbalEVA>() != null ? CoupleTrigger.Kerbal : CoupleTrigger.Other;

            System.MessageSender.SendVesselCouple(partFrom.vessel, partTo.flightID, removedVesselId, partFrom.flightID, trigger);

            var ownFinalVessel = LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            if (ownFinalVessel)
            {
                foreach (var kerbal in partFrom.vessel.GetVesselCrew())
                {
                    LockSystem.Singleton.AcquireKerbalLock(kerbal.name, true);
                }

                JumpIfVesselOwnerIsInFuture(removedVesselId);
            }
            else
            {
                JumpIfVesselOwnerIsInFuture(partFrom.vessel.id);
            }

            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(removedVesselId, false);
            VesselRemoveSystem.Singleton.DelayedKillVessel(removedVesselId, false, "Killing coupled vessel during a detected coupling", 500);
            LockSystem.Singleton.ReleaseAllVesselLocks(null, removedVesselId);
        }


        /// <summary>
        /// Jumps to the subspace of the controller vessel in case he is more advanced in time
        /// </summary>
        private static void JumpIfVesselOwnerIsInFuture(Guid vesselId)
        {
            var dominantVesselOwner = LockSystem.LockQuery.GetControlLockOwner(vesselId);
            if (dominantVesselOwner != null)
            {
                var dominantVesselOwnerSubspace = WarpSystem.Singleton.GetPlayerSubspace(dominantVesselOwner);
                WarpSystem.Singleton.WarpIfSubspaceIsMoreAdvanced(dominantVesselOwnerSubspace);
            }
        }
    }
}

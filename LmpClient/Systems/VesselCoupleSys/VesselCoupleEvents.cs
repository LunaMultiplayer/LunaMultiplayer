using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using System;

namespace LmpClient.Systems.VesselCoupleSys
{
    public class VesselCoupleEvents : SubSystem<VesselCoupleSystem>
    {
        public void CoupleStart(Part partFrom, Part partTo)
        {
            if (VesselCommon.IsSpectating) return;

            //If neither the vessel 1 or vessel2 locks belong to us, ignore the coupling
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(partTo.vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Detected part couple! Part: {partFrom.partName} Vessel: {partFrom.vessel.id} - CoupledPart: {partTo.partName} CoupledVessel: {partTo.vessel.id}");
        }

        public void CoupleComplete(Part partFrom, Part partTo, Guid removedVesselId)
        {
            if (VesselCommon.IsSpectating) return;

            //If neither the vessel 1 or vessel2 locks belong to us, ignore the coupling
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(partFrom.vessel.id, SettingsSystem.CurrentSettings.PlayerName) &&
                !LockSystem.LockQuery.UpdateLockBelongsToPlayer(removedVesselId, SettingsSystem.CurrentSettings.PlayerName)) return;

            LunaLog.Log($"Couple complete! Removed vessel: {removedVesselId}");
            //Yes, it doesn't make much sense but that's the correct order of the parameters...
            System.MessageSender.SendVesselCouple(partFrom.vessel, partTo.flightID, removedVesselId, partFrom.flightID);

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

            VesselRemoveSystem.Singleton.DelayedKillVessel(removedVesselId, false, "Killing coupled vessel during a detected coupling", 500);
            VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(removedVesselId, false);
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

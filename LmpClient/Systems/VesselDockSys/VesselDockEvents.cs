using LmpClient.Base;
using LmpClient.Systems.Lock;
using LmpClient.Systems.VesselProtoSys;
using LmpClient.Systems.VesselRemoveSys;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Time;
using System;
using VesselCommon = LmpClient.VesselUtilities.VesselCommon;

namespace LmpClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        private static bool _ownDominantVessel;
        private static bool _ownWeakVessel;

        /// <summary>
        /// Called just before the docking sequence starts
        /// </summary>
        public void OnVesselDocking(uint vessel1PersistentId, uint vessel2PersistentId)
        {
            if (VesselCommon.IsSpectating) return;

            if (!FlightGlobals.PersistentVesselIds.TryGetValue(vessel1PersistentId, out var vessel1) ||
                !FlightGlobals.PersistentVesselIds.TryGetValue(vessel2PersistentId, out var vessel2))
                return;

            if (vessel1.isEVA || vessel2.isEVA) return;

            CurrentDockEvent.DockingTime = LunaNetworkTime.UtcNow;

            var dominantVessel = Vessel.GetDominantVessel(vessel1, vessel2);
            CurrentDockEvent.DominantVesselId = dominantVessel.id;

            var weakVessel = dominantVessel == vessel1 ? vessel2 : vessel1;
            CurrentDockEvent.WeakVesselId = weakVessel.id;

            _ownDominantVessel = FlightGlobals.ActiveVessel == dominantVessel;
            _ownWeakVessel = FlightGlobals.ActiveVessel == weakVessel;

            LunaLog.Log(FlightGlobals.ActiveVessel
                ? $"Docking detected! Dominant: {dominantVessel.id} Weak: {weakVessel.id} Current {FlightGlobals.ActiveVessel.id}"
                : $"Docking detected! Dominant: {dominantVessel.id} Weak: {weakVessel.id}");
        }

        public void OnDockingComplete(GameEvents.FromToAction<Part, Part> data)
        {
            //Do not do a "VesselCommon.IsSpectating" check as when we detect the dock and we own the weak vessel, we switch to the dominant and we will be spectating!

            if (data.from.vessel.isEVA || data.from.vessel.isEVA) return;

            LunaLog.Log(_ownDominantVessel || _ownWeakVessel
                ? $"[LMP]: Docking finished! We own the {(_ownDominantVessel ? "DOMINANT" : "WEAK")} vessel"
                : "[LMP]: Docking finished! We don't own anything");
            
            if (_ownDominantVessel)
            {
                System.MessageSender.SendDockInformation(CurrentDockEvent.WeakVesselId, FlightGlobals.ActiveVessel, WarpSystem.Singleton.CurrentSubspace);
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel);

                foreach (var kerbal in FlightGlobals.ActiveVessel.GetVesselCrew())
                {
                    LockSystem.Singleton.AcquireKerbalLock(kerbal.name, true);
                }

                JumpIfVesselOwnerIsInFuture(CurrentDockEvent.WeakVesselId);
            }
            else if (_ownWeakVessel)
            {
                CoroutineUtil.StartDelayedRoutine("OnDockingComplete", () => System.MessageSender.SendDockInformation(CurrentDockEvent.WeakVesselId,
                            FlightGlobals.ActiveVessel, WarpSystem.Singleton.CurrentSubspace), 3);

                JumpIfVesselOwnerIsInFuture(CurrentDockEvent.DominantVesselId);
            }

            if (_ownDominantVessel || _ownWeakVessel)
            {
                CoroutineUtil.StartDelayedRoutine("OnDockingComplete", () =>
                {
                    VesselRemoveSystem.Singleton.KillVessel(CurrentDockEvent.WeakVesselId, true, "Killing weak vessel during a detected docking");
                }, 3);

                LockSystem.Singleton.ReleaseAllVesselLocks(null, CurrentDockEvent.WeakVesselId);

                VesselRemoveSystem.Singleton.MessageSender.SendVesselRemove(CurrentDockEvent.WeakVesselId, false);
            }
        }

        /// <summary>
        /// Event called just when the undocking starts
        /// </summary>
        public void UndockingStart(Part undockingPart)
        {
            CurrentUndockEvent.UndockingVesselId = undockingPart.vessel.id;
        }

        /// <summary>
        /// Event called after the undocking is completed and we have the 2 final vessels
        /// </summary>
        public void UndockingComplete(Vessel vessel1, Vessel vessel2)
        {
            if (VesselCommon.IsSpectating) return;
            
            LunaLog.Log(FlightGlobals.ActiveVessel
                ? $"Undock detected! Current {FlightGlobals.ActiveVessel.id} Vessel1 {vessel1.id} Vessel2 {vessel2.id}" 
                : $"Undock detected! Vessel1 {vessel1.id} Vessel2 {vessel2.id}");

            //Send the definitions of the new vessels once their orbits are initialized

            LunaLog.Log($"Sending undocked vessel1 {vessel1.id}");
            LockSystem.Singleton.AcquireUpdateLock(vessel1.id);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel1);

            LunaLog.Log($"Sending undocked vessel2 {vessel2.id}");
            LockSystem.Singleton.AcquireUpdateLock(vessel2.id);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel2);

            LunaLog.Log("Undocking finished");
        }

        /// <summary>
        /// After an undock, once both vessels have their orbits ready, send them to the server
        /// </summary>
        public void LmpVesselReady(Vessel vessel)
        {
            


        }

        #region Private

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

        #endregion
    }
}

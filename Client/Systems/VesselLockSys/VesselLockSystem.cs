using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Utilities;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselLockSys
{
    /// <summary>
    /// This class handles the locks in the vessel
    /// </summary>
    public class VesselLockSystem : System<VesselLockSystem>
    {
        #region Fields & properties

        public const string SpectateLock = "LMP_Spectating";
        private ScreenMessage _spectateMessage;

        private string GetVesselOwner => VesselCommon.IsSpectating ? LockSystem.Singleton.LockOwner("control-" + FlightGlobals.ActiveVessel.id) : "";

        private VesselLockEvents VesselMainEvents { get; } = new VesselLockEvents();

        private bool VesselLockSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f &&
            FlightGlobals.ActiveVessel != null;

        private string SpectatingMessage => VesselCommon.IsSpectating ? $"This vessel is being controlled by {GetVesselOwner}." : "";

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onLevelWasLoadedGUIReady.Add(VesselMainEvents.OnSceneChanged);
            GameEvents.onVesselChange.Add(VesselMainEvents.OnVesselChange);
            SetupRoutine(new RoutineDefinition(3000, RoutineExecution.Update, TryGetControlLock));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateSecondaryVesselsLocks));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateOnScreenSpectateMessage));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onLevelWasLoadedGUIReady.Remove(VesselMainEvents.OnSceneChanged);
            GameEvents.onVesselChange.Remove(VesselMainEvents.OnVesselChange);
        }

        #endregion

        #region Update methods

        /// <summary>
        /// In case the player who control the ship drops the control, here we try to get it.
        /// </summary>
        private void TryGetControlLock()
        {
            if (Enabled && VesselLockSystemReady && VesselCommon.IsSpectating)
            {
                if (!LockSystem.Singleton.LockExists("control-" + FlightGlobals.ActiveVessel.id))
                {
                    //Don't force as maybe other players are spectating too so the fastests is the winner :)
                    StopSpectatingAndGetControl(FlightGlobals.ActiveVessel, false);
                }
            }
        }

        /// <summary>
        /// After some ms get the update lock for vessels that are close to us (not packed and not ours) not dead and that nobody has the update lock
        /// </summary>
        private void UpdateSecondaryVesselsLocks()
        {
            if (Enabled && VesselLockSystemReady)
            {
                var validSecondaryVessels = GetValidSecondaryVesselIds().ToArray();
                foreach (var checkVessel in validSecondaryVessels)
                {
                    //Don't force it as maybe another player sent this request aswell
                    LockSystem.Singleton.AcquireLock("update-" + checkVessel);
                }

                var vesselsToRelease = GetSecondaryVesselIdsThatShouldBeReleased().ToArray();
                foreach (var releaseVessel in vesselsToRelease)
                {
                    LockSystem.Singleton.ReleaseLock("update-" + releaseVessel);
                }
            }
        }

        /// <summary>
        /// Show a message on the screen if we are spectating
        /// </summary>
        private void UpdateOnScreenSpectateMessage()
        {
            if (Enabled && VesselLockSystemReady)
            {
                if (VesselCommon.IsSpectating)
                {
                    if (_spectateMessage != null)
                        _spectateMessage.duration = 0f;
                    _spectateMessage = ScreenMessages.PostScreenMessage(SpectatingMessage, 1000 * 2, ScreenMessageStyle.UPPER_CENTER);
                }
                else
                {
                    if (_spectateMessage != null)
                    {
                        _spectateMessage.duration = 0f;
                        _spectateMessage = null;
                    }
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Drop the control locks of other vessels except the active
        /// </summary>
        public void DropAllOtherVesselControlLocks()
        {
            var activeVesselId = FlightGlobals.ActiveVessel?.id;
            if (activeVesselId.HasValue)
                LockSystem.Singleton.ReleaseControlLocksExcept($"control-{activeVesselId}");
        }

        /// <summary>
        /// Start expectating
        /// </summary>
        public void StartSpectating()
        {
            InputLockManager.SetControlLock(LmpGuiUtil.BlockAllControls, SpectateLock);
            if (FlightGlobals.ActiveVessel != null)
                LockSystem.Singleton.AcquireSpectatorLock(FlightGlobals.ActiveVessel.id);
            VesselCommon.IsSpectating = true;
        }
        
        public void StopSpectating()
        {
            InputLockManager.RemoveControlLock(SpectateLock);
            LockSystem.Singleton.ReleaseSpectatorLock();
            VesselCommon.IsSpectating = false;
        }

        /// <summary>
        /// Stop spectating and get control of the given vessel
        /// </summary>
        public void StopSpectatingAndGetControl(Vessel vessel, bool force)
        {
            LockSystem.Singleton.AcquireLock("update-" + vessel.id, force);
            if (!LockSystem.Singleton.LockIsOurs("control-" + vessel.id))
            {
                LockSystem.Singleton.AcquireLock("control-" + vessel.id, force);
            }

            if (VesselCommon.IsSpectating)
            {
                StopSpectating();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Return the OTHER vessel ids of the vessels that are loaded (close to us) not dead and not in safety bubble.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetValidSecondaryVesselIds()
        {
            return FlightGlobals.Vessels
                .Where(v => v.loaded && v.state != Vessel.State.DEAD &&
                            (v.id != FlightGlobals.ActiveVessel.id) &&
                            !VesselCommon.IsInSafetyBubble(v) &&
                            !LockSystem.Singleton.LockExists("update-" + v.id))
                .Select(v => v.id);
        }

        /// <summary>
        /// Return the vessel ids of the OTHER vessels that are far, dead, in safety bubble, and being updated by us.
        /// We use this list to relase the locks as we shouldn't update them
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetSecondaryVesselIdsThatShouldBeReleased()
        {
            return FlightGlobals.Vessels
                .Where(v => v.id != FlightGlobals.ActiveVessel.id &&
                            LockSystem.Singleton.LockIsOurs("update-" + v.id) &&
                            (!v.loaded || v.state == Vessel.State.DEAD ||
                            VesselCommon.IsInSafetyBubble(v)))
                .Select(v => v.id);
        }

        #endregion
    }
}

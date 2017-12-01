using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselLockSys
{
    /// <summary>
    /// This class handles the locks in the vessel
    /// </summary>
    public class VesselLockSystem : Base.System
    {
        #region Fields & properties

        public const string SpectateLock = "LMP_Spectating";
        private ScreenMessage _spectateMessage;

        private string GetVesselOwner => VesselCommon.IsSpectating ?
            LockSystem.LockQuery.GetControlLockOwner(FlightGlobals.ActiveVessel.id) :
            "";

        private VesselLockEvents VesselMainEvents { get; } = new VesselLockEvents();

        private bool VesselLockSystemReady => Enabled && HighLogic.LoadedSceneIsFlight && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f &&
            FlightGlobals.ActiveVessel != null;

        private string SpectatingMessage => VesselCommon.IsSpectating ? $"This vessel is being controlled by {GetVesselOwner}." : "";

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselChange.Add(VesselMainEvents.OnVesselChange);
            SetupRoutine(new RoutineDefinition(3000, RoutineExecution.Update, TryGetControlLock));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateSecondaryVesselsLocks));
            SetupRoutine(new RoutineDefinition(2000, RoutineExecution.Update, UpdateUnloadedVesselsLocks));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateOnScreenSpectateMessage));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselChange.Remove(VesselMainEvents.OnVesselChange);
        }

        #endregion

        #region Update methods

        /// <summary>
        /// In case the player who control the ship drops the control, here we try to get it.
        /// </summary>
        private void TryGetControlLock()
        {
            if (Enabled && VesselLockSystemReady)
            {
                if (VesselCommon.IsSpectating)
                {
                    if (!LockSystem.LockQuery.ControlLockExists(FlightGlobals.ActiveVessel.id))
                    {
                        //Don't force as maybe other players are spectating too so the fastests is the winner :)
                        StopSpectatingAndGetControl(FlightGlobals.ActiveVessel, false);
                    }
                }
                else
                {
                    if (!LockSystem.LockQuery.ControlLockBelongsToPlayer(FlightGlobals.ActiveVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                    {
                        GetAllLocksForVessel(FlightGlobals.ActiveVessel, true);
                    }
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
                var validSecondaryVessels = GetValidSecondaryVesselIds();
                foreach (var checkVessel in validSecondaryVessels)
                {
                    //Don't force it as maybe another player sent this request aswell
                    SystemsContainer.Get<LockSystem>().AcquireUpdateLock(checkVessel);
                }

                var vesselIdsWithUpdateLocks = GetVesselIdsWeCurrentlyUpdate();
                foreach (var vesselId in vesselIdsWithUpdateLocks)
                {
                    if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                        continue;

                    //For all the vessels we are updating we FORCE the unloaded update lock if we don't have it.
                    SystemsContainer.Get<LockSystem>().AcquireUnloadedUpdateLock(vesselId, true);
                }

                var vesselsToRelease = GetSecondaryVesselIdsThatShouldBeReleased();
                foreach (var releaseVessel in vesselsToRelease)
                {
                    SystemsContainer.Get<LockSystem>().ReleaseUpdateLock(releaseVessel);
                }
            }
        }

        /// <summary>
        /// After some ms get the unloaded update lock for vessels that are far to us (not loaded) not dead and that nobody has the update lock
        /// </summary>
        private void UpdateUnloadedVesselsLocks()
        {
            if (Enabled && VesselLockSystemReady)
            {
                var validSecondaryUnloadedVessels = GetValidUnloadedVesselIds();
                foreach (var checkVessel in validSecondaryUnloadedVessels)
                {
                    //Don't force it as maybe another player sent this request aswell
                    SystemsContainer.Get<LockSystem>().AcquireUnloadedUpdateLock(checkVessel);
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
                SystemsContainer.Get<LockSystem>().ReleaseControlLocksExcept(activeVesselId.Value);
        }

        /// <summary>
        /// Start expectating
        /// </summary>
        public void StartSpectating()
        {
            InputLockManager.SetControlLock(LmpGuiUtil.BlockAllControls, SpectateLock);
            var currentSpectatorLock = LockSystem.LockQuery.GetSpectatorLock(SettingsSystem.CurrentSettings.PlayerName);
            if (FlightGlobals.ActiveVessel != null && currentSpectatorLock == null)
                SystemsContainer.Get<LockSystem>().AcquireSpectatorLock(FlightGlobals.ActiveVessel.id);
            VesselCommon.IsSpectating = true;
            HighLogic.CurrentGame.Parameters.Flight.CanEVA = false;
        }

        public void StopSpectating()
        {
            InputLockManager.RemoveControlLock(SpectateLock);
            SystemsContainer.Get<LockSystem>().ReleaseSpectatorLock();
            VesselCommon.IsSpectating = false;
            HighLogic.CurrentGame.Parameters.Flight.CanEVA = true;
        }

        /// <summary>
        /// Stop spectating and get control of the given vessel
        /// </summary>
        public void StopSpectatingAndGetControl(Vessel vessel, bool force)
        {
            GetAllLocksForVessel(vessel, force);

            if (VesselCommon.IsSpectating)
            {
                StopSpectating();
            }
        }


        #endregion

        #region Private methods

        /// <summary>
        /// Get all the locks of a vessel
        /// </summary>
        private static void GetAllLocksForVessel(Vessel vessel, bool force)
        {
            if (!LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                SystemsContainer.Get<LockSystem>().AcquireControlLock(vessel.id, force);
            if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                SystemsContainer.Get<LockSystem>().AcquireUpdateLock(vessel.id, force);
            if (!LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                SystemsContainer.Get<LockSystem>().AcquireUnloadedUpdateLock(vessel.id, force);
        }

        /// <summary>
        /// Return the vessel ids of the vessels where we have an update lock
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetVesselIdsWeCurrentlyUpdate()
        {
            return LockSystem.LockQuery
                .GetAllUpdateLocks(SettingsSystem.CurrentSettings.PlayerName)
                .Select(l => l.VesselId);
        }

        /// <summary>
        /// Return the OTHER vessel ids of the vessels that are unloadedloaded not dead and not in safety bubble.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetValidUnloadedVesselIds()
        {
            return FlightGlobals.Vessels
                .Where(v => v != null && v.state != Vessel.State.DEAD && !v.loaded && v.vesselType != VesselType.Flag &&
                            v.id != FlightGlobals.ActiveVessel?.id &&
                            !VesselCommon.IsInSafetyBubble(v) &&
                            !LockSystem.LockQuery.UnloadedUpdateLockExists(v.id) &&
                            !LockSystem.LockQuery.UpdateLockExists(v.id))
                .Select(v => v.id);
        }

        /// <summary>
        /// Return the OTHER vessel ids of the vessels that are loaded (close to us) not dead and not in safety bubble.
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetValidSecondaryVesselIds()
        {
            return FlightGlobals.VesselsLoaded
                .Where(v => v != null && v.state != Vessel.State.DEAD && v.vesselType != VesselType.Flag &&
                            v.id != FlightGlobals.ActiveVessel?.id &&
                            !VesselCommon.IsInSafetyBubble(v) &&
                            !LockSystem.LockQuery.UpdateLockExists(v.id))
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
                .Where(v => v.id != FlightGlobals.ActiveVessel?.id &&
                            LockSystem.LockQuery.UpdateLockBelongsToPlayer(v.id, SettingsSystem.CurrentSettings.PlayerName) &&
                            (!v.loaded || v.state == Vessel.State.DEAD ||
                            VesselCommon.IsInSafetyBubble(v)))
                .Select(v => v.id);
        }

        #endregion
    }
}

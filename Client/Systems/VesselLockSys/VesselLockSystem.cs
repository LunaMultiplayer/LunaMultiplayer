using System;
using System.Collections.Generic;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.Warp;
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
        private float _lastSpectateMessageUpdate;
        private const float UpdateScreenMessageInterval = 1f;

        private bool _isSpectating;
        public bool IsSpectating
        {
            get { return HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel != null && _isSpectating; }
            set { _isSpectating = value; }
        }

        private string GetVesselOwner => IsSpectating ? LockSystem.Singleton.LockOwner("control-" + FlightGlobals.ActiveVessel.id) : "";

        private VesselLockEvents VesselMainEvents { get; } = new VesselLockEvents();

        private bool _enabled;
        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                    RegisterGameHooks();
                else if (_enabled && !value)
                    UnregisterGameHooks();

                _enabled = value;
            }
        }

        private bool VesselLockSystemReady => Enabled && HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ready && Time.timeSinceLevelLoad > 1f && FlightGlobals.ActiveVessel != null;

        private string SpectatingMessage { get; set; }

        private const int CheckSecondaryVesselsMsInterval = 500;
        private long _lastCheckTime;

        #endregion

        #region base overrides

        public override void Update()
        {
            if (!VesselLockSystemReady)
                return;

            UpdateOnScreenSpectateMessage();

            if (IsSpectating)
            {
                CheckWarp();
                TryGetControlLock();
            }
            else
                UpdateSecondaryVesselsLocks();
        }

        /// <summary>
        /// If the player we are expectating warps, then stop spectating
        /// </summary>
        private void CheckWarp()
        {
            if (WarpSystem.Singleton.CurrentSubspace != WarpSystem.Singleton.GetPlayerSubspace(GetVesselOwner))
                SpectatingMessage = $"The player you were spectating ({GetVesselOwner}) warped";
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
            SpectatingMessage = $"This vessel is being controlled by {GetVesselOwner}.";
            InputLockManager.SetControlLock(LmpGuiUtil.BlockAllControls, SpectateLock);
            IsSpectating = true;
        }

        /// <summary>
        /// Stop spectating and get control of the given vessel
        /// </summary>
        public void StopSpectatingAndGetControl(Vessel vessel, bool force = true)
        {
            LockSystem.Singleton.AcquireLock("update-" + vessel.id, force);
            if (!LockSystem.Singleton.LockIsOurs("control-" + vessel.id))
            {
                LockSystem.Singleton.AcquireLock("control-" + vessel.id, force);
            }
            
            if (IsSpectating)
            {
                InputLockManager.RemoveControlLock(SpectateLock);
                IsSpectating = false;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// In case the player who control the ship drops the control, here we try to get it.
        /// </summary>
        private void TryGetControlLock()
        {
            if (!LockSystem.Singleton.LockExists("control-" + FlightGlobals.ActiveVessel.id))
            {
                //Don't force as maybe other players are spectating too so the fastests is the winner :)
                StopSpectatingAndGetControl(FlightGlobals.ActiveVessel, false);
            }
        }

        private void RegisterGameHooks()
        {
            GameEvents.onLevelWasLoadedGUIReady.Add(VesselMainEvents.OnSceneChanged);
            GameEvents.onVesselChange.Add(VesselMainEvents.OnVesselChange);
        }

        private void UnregisterGameHooks()
        {
            GameEvents.onLevelWasLoadedGUIReady.Remove(VesselMainEvents.OnSceneChanged);
            GameEvents.onVesselChange.Remove(VesselMainEvents.OnVesselChange);
        }

        /// <summary>
        /// After some ms get the update lock for vessels that are close to us (not packed and not ours) not dead and that nobody has the update lock
        /// </summary>
        private void UpdateSecondaryVesselsLocks()
        {
            if (DateTime.Now.Ticks - _lastCheckTime >= TimeSpan.FromMilliseconds(CheckSecondaryVesselsMsInterval).Ticks)
            {
                _lastCheckTime = DateTime.Now.Ticks;
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
        /// Return the OTHER vessel ids of the vessels that are loaded and not packed (close to us) not dead, not in safety bubble,
        /// with a update lock that is free.
        /// Basically a close vessel without a updater
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Guid> GetValidSecondaryVesselIds()
        {
            return FlightGlobals.Vessels
                .Where(v => v.loaded && !v.packed && v.state != Vessel.State.DEAD &&
                            (v.id != FlightGlobals.ActiveVessel.id) &&
                            !VesselCommon.IsInSafetyBubble(v.GetWorldPos3D(), v.mainBody) &&
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
                            (!v.loaded || v.packed || v.state == Vessel.State.DEAD ||
                            VesselCommon.IsInSafetyBubble(v.GetWorldPos3D(), v.mainBody)))
                .Select(v => v.id);
        }

        /// <summary>
        /// Show a message on the screen if we are spectating
        /// </summary>
        private void UpdateOnScreenSpectateMessage()
        {
            if (Time.realtimeSinceStartup - _lastSpectateMessageUpdate > UpdateScreenMessageInterval)
            {
                _lastSpectateMessageUpdate = Time.realtimeSinceStartup;
                if (IsSpectating)
                {
                    if (_spectateMessage != null)
                        _spectateMessage.duration = 0f;
                    _spectateMessage = ScreenMessages.PostScreenMessage(SpectatingMessage, UpdateScreenMessageInterval * 2, ScreenMessageStyle.UPPER_CENTER);
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
    }
}

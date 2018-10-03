using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Localization;
using LmpClient.Systems.Lock;
using LmpClient.VesselUtilities;
using LmpCommon.Locks;
using System;

namespace LmpClient.Systems.VesselLockSys
{
    /// <summary>
    /// This class handles the locks in the vessel
    /// </summary>
    public class VesselLockSystem : System<VesselLockSystem>
    {
        #region Fields & properties

        public const string SpectateLock = "LMP_Spectating";
        public const ControlTypes BlockAllControls = ControlTypes.ALLBUTCAMERAS ^ ControlTypes.MAP ^ ControlTypes.PAUSE ^
                                                     ControlTypes.APPLAUNCHER_BUTTONS ^ ControlTypes.VESSEL_SWITCHING ^ ControlTypes.GUI;

        private ScreenMessage _spectateMessage;

        private string GetVesselOwner => VesselCommon.IsSpectating ?
            LockSystem.LockQuery.GetControlLockOwner(FlightGlobals.ActiveVessel.id) :
            "";

        private VesselLockEvents VesselLockEvents { get; } = new VesselLockEvents();
        
        private string SpectatingMessage => VesselCommon.IsSpectating ? LocalizationContainer.ScreenText.Spectating + $" {GetVesselOwner}." : "";

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselLockSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselChange.Add(VesselLockEvents.OnVesselChange);
            GameEvents.onLevelWasLoadedGUIReady.Add(VesselLockEvents.LevelLoaded);
            GameEvents.onVesselLoaded.Add(VesselLockEvents.VesselLoaded);
            LockEvent.onLockAcquireUnityThread.Add(VesselLockEvents.LockAcquire);
            LockEvent.onLockReleaseUnityThread.Add(VesselLockEvents.LockReleased);
            VesselUnloadEvent.onVesselUnloading.Add(VesselLockEvents.VesselUnloading);
            FlightDriverEvent.onFlightStarted.Add(VesselLockEvents.FlightStarted);

            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, UpdateOnScreenSpectateMessage));
            SetupRoutine(new RoutineDefinition(10000, RoutineExecution.Update, LockSystem.Singleton.MessageSender.SendLocksRequest));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselChange.Remove(VesselLockEvents.OnVesselChange);
            GameEvents.onLevelWasLoadedGUIReady.Remove(VesselLockEvents.LevelLoaded);
            GameEvents.onVesselLoaded.Remove(VesselLockEvents.VesselLoaded);
            LockEvent.onLockAcquireUnityThread.Remove(VesselLockEvents.LockAcquire);
            LockEvent.onLockReleaseUnityThread.Remove(VesselLockEvents.LockReleased);
            VesselUnloadEvent.onVesselUnloading.Remove(VesselLockEvents.VesselUnloading);
            FlightDriverEvent.onFlightStarted.Remove(VesselLockEvents.FlightStarted);
        }

        #endregion

        #region Update methods
        
        /// <summary>
        /// Show a message on the screen if we are spectating
        /// </summary>
        private void UpdateOnScreenSpectateMessage()
        {
            if (VesselCommon.IsSpectating)
            {
                if (_spectateMessage != null)
                    _spectateMessage.duration = 0f;
                _spectateMessage = LunaScreenMsg.PostScreenMessage(SpectatingMessage, 1000 * 2, ScreenMessageStyle.UPPER_CENTER);
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

        #endregion

        #region Public methods

        public void StartSpectating(Guid spectatingVesselId)
        {
            //Lock all vessel controls
            InputLockManager.SetControlLock(BlockAllControls, SpectateLock);

            LockSystem.Singleton.ReleasePlayerLocks(LockType.Spectator);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Kerbal);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.Update);
            LockSystem.Singleton.ReleasePlayerLocks(LockType.UnloadedUpdate);

            LockSystem.Singleton.AcquireSpectatorLock(FlightGlobals.ActiveVessel.id);

            VesselCommon.IsSpectating = true;
            VesselCommon.SpectatingVesselId = spectatingVesselId;

            //Disable "EVA" button
            HighLogic.CurrentGame.Parameters.Flight.CanEVA = false;
            SpectateEvent.onStartSpectating.Fire();
        }

        public void StopSpectating()
        {
            InputLockManager.RemoveControlLock(SpectateLock);
            LockSystem.Singleton.ReleaseSpectatorLock();
            VesselCommon.IsSpectating = false;

            if (HighLogic.CurrentGame?.Parameters?.Flight != null)
                HighLogic.CurrentGame.Parameters.Flight.CanEVA = true;

            SpectateEvent.onFinishedSpectating.Fire();
        }

        #endregion
    }
}

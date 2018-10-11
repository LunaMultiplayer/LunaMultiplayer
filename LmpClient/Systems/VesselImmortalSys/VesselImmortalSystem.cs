using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;

namespace LmpClient.Systems.VesselImmortalSys
{
    /// <summary>
    /// This class makes the other vessels immortal, this way if we crash against them they are not destroyed but we do.
    /// In the other player screens they will be destroyed and they will send their new vessel definition.
    /// </summary>
    public class VesselImmortalSystem : System<VesselImmortalSystem>
    {
        #region Fields & properties

        public static VesselImmortalEvents VesselImmortalEvents { get; } = new VesselImmortalEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselImmortalSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            RailEvent.onVesselGoneOnRails.Add(VesselImmortalEvents.VesselGoOnRails);
            RailEvent.onVesselGoneOffRails.Add(VesselImmortalEvents.VesselGoOffRails);
            GameEvents.onVesselPartCountChanged.Add(VesselImmortalEvents.PartCountChanged);
            GameEvents.onVesselChange.Add(VesselImmortalEvents.OnVesselChange);
            SpectateEvent.onStartSpectating.Add(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Add(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Add(VesselImmortalEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Add(VesselImmortalEvents.OnLockRelease);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            RailEvent.onVesselGoneOnRails.Remove(VesselImmortalEvents.VesselGoOnRails);
            RailEvent.onVesselGoneOffRails.Remove(VesselImmortalEvents.VesselGoOffRails);
            GameEvents.onVesselPartCountChanged.Remove(VesselImmortalEvents.PartCountChanged);
            GameEvents.onVesselChange.Remove(VesselImmortalEvents.OnVesselChange);
            SpectateEvent.onStartSpectating.Remove(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Remove(VesselImmortalEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Remove(VesselImmortalEvents.OnLockRelease);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sets the immortal state based on the lock you have on that vessel
        /// </summary>
        public void SetImmortalStateBasedOnLock(Vessel vessel)
        {
            if (vessel == null) return;

            var isOurs = LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                         LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                         !LockSystem.LockQuery.UpdateLockExists(vessel.id);

            vessel.SetImmortal(!isOurs);
        }

        #endregion
    }
}

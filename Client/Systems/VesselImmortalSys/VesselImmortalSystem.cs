using LunaClient.Events;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;

namespace LunaClient.Systems.VesselImmortalSys
{
    /// <summary>
    /// This class makes the other vessels immortal, this way if we crash against them they are not destroyed but we do.
    /// In the other player screens they will be destroyed and they will send their new vessel definition.
    /// </summary>
    public class VesselImmortalSystem : Base.System<VesselImmortalSystem>
    {
        #region Fields & properties

        public static VesselImmortalEvents VesselImmortalEvents { get; } = new VesselImmortalEvents();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselImmortalSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselGoOnRails.Add(VesselImmortalEvents.VesselGoOnRails);
            GameEvents.onVesselGoOffRails.Add(VesselImmortalEvents.VesselGoOffRails);
            GameEvents.onVesselLoaded.Add(VesselImmortalEvents.VesselLoaded);
            SpectateEvent.onStartSpectating.Add(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Add(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Add(VesselImmortalEvents.OnLockAcquire);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselGoOnRails.Remove(VesselImmortalEvents.VesselGoOnRails);
            GameEvents.onVesselGoOffRails.Remove(VesselImmortalEvents.VesselGoOffRails);
            GameEvents.onVesselLoaded.Remove(VesselImmortalEvents.VesselLoaded);
            SpectateEvent.onStartSpectating.Remove(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Remove(VesselImmortalEvents.OnLockAcquire);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Sets the immortal state based on the lock you have on that vessel
        /// </summary>
        public void SetImmortalStateBasedOnLock(Vessel vessel)
        {
            if (vessel == null) return;

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == vessel.id)
                return;

            var isOurs = LockSystem.LockQuery.ControlLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                         LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName) ||
                         !LockSystem.LockQuery.UpdateLockExists(vessel.id);

            SetVesselImmortalState(vessel, !isOurs);
        }

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        public void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            if (vessel == null || !vessel.loaded) return;

            foreach (var part in vessel.Parts)
            {
                if (part == null) return;

                //Do not remove the colliders as then you can't dock
                //if(part.collider != null)
                //    part.collider.enabled = SettingsSystem.CurrentSettings.CollidersEnabled || !immortal;

                part.gTolerance = immortal ? double.PositiveInfinity : 50;
                part.maxPressure = immortal ? double.PositiveInfinity : 4000;

                part.crashTolerance = immortal ? float.PositiveInfinity : 9f;

                if (part.collisionEnhancer != null)
                    part.collisionEnhancer.OnTerrainPunchThrough = immortal ? CollisionEnhancerBehaviour.DO_NOTHING : CollisionEnhancerBehaviour.EXPLODE;

                if (immortal)
                    part.attachJoint?.SetUnbreakable(true, true);
                else
                    part.ResetJoints();

                //Do not set this as then you can't click on parts
                //part.SetDetectCollisions(!immortal);
            }

            if (immortal)
            {
                ImmortalEvent.onVesselImmortal.Fire(vessel);
            }
            else
            {
                ImmortalEvent.onVesselMortal.Fire(vessel);
            }
        }

        #endregion
    }
}

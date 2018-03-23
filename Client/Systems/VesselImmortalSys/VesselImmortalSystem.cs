using LunaClient.Events;

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
            GameEvents.onVesselLoaded.Add(VesselImmortalEvents.VesselLoaded);
            SpectateEvent.onStartSpectating.Add(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Add(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Add(VesselImmortalEvents.OnLockAcquire);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselLoaded.Remove(VesselImmortalEvents.VesselLoaded);
            SpectateEvent.onStartSpectating.Remove(VesselImmortalEvents.StartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(VesselImmortalEvents.FinishSpectating);
            LockEvent.onLockAcquireUnityThread.Remove(VesselImmortalEvents.OnLockAcquire);
        }

        #endregion
        
        #region public methods

        /// <summary>
        /// Set all vessel parts to unbreakable or not (makes the vessel immortal or not)
        /// </summary>
        public void SetVesselImmortalState(Vessel vessel, bool immortal)
        {
            if (vessel == null || !vessel.loaded) return;

            foreach (var part in vessel.Parts)
            {
                if (part.attachJoint != null)
                    part.attachJoint?.SetUnbreakable(immortal, part.rigidAttachment);

                //Do not remove the colliders as then you can't dock
                //if(part.collider != null)
                //    part.collider.enabled = SettingsSystem.CurrentSettings.CollidersEnabled || !immortal;

                part.gTolerance = immortal ? double.MaxValue : 50;
                part.maxPressure = immortal ? double.MaxValue : 4000;

                part.crashTolerance = immortal ? float.MaxValue : 9f;

                if(part.collisionEnhancer != null)
                    part.collisionEnhancer.OnTerrainPunchThrough = immortal ? CollisionEnhancerBehaviour.DO_NOTHING : CollisionEnhancerBehaviour.EXPLODE;

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

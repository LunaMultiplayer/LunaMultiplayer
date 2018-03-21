using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Systems.Lock;

namespace LunaClient.Systems.KscScene
{
    /// <summary>
    /// This class controls what happens when we are in KSC screen
    /// </summary>
    public class KscSceneSystem : System<KscSceneSystem>
    {
        private static KscSceneEvents KscSceneEvents { get; } = new KscSceneEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(KscSceneSystem);

        public bool TriggerMarkersRefresh { get; set; }

        protected override void OnEnabled()
        {
            LockSystem.Singleton.RegisterAcquireHook(KscSceneEvents.OnLockAcquire);
            LockSystem.Singleton.RegisterReleaseHook(KscSceneEvents.OnLockRelease);
            SetupRoutine(new RoutineDefinition(50, RoutineExecution.Update, CheckIfRefreshMarkersIsNeeded));
        }

        private void CheckIfRefreshMarkersIsNeeded()
        {
            if (TriggerMarkersRefresh)
            {
                KSCVesselMarkers.fetch?.RefreshMarkers();
                TriggerMarkersRefresh = false;
            }
        }

        protected override void OnDisabled()
        {
            LockSystem.Singleton.UnregisterAcquireHook(KscSceneEvents.OnLockAcquire);
            LockSystem.Singleton.UnregisterReleaseHook(KscSceneEvents.OnLockRelease);
        }

        #endregion
    }
}

using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Events;

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
            LockEvent.onLockAcquire.Add(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockRelease.Add(KscSceneEvents.OnLockRelease);
            SetupRoutine(new RoutineDefinition(50, RoutineExecution.Update, CheckIfRefreshMarkersIsNeeded));
        }

        protected override void OnDisabled()
        {
            LockEvent.onLockAcquire.Remove(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockRelease.Remove(KscSceneEvents.OnLockRelease);
        }

        #endregion

        private void CheckIfRefreshMarkersIsNeeded()
        {
            if (TriggerMarkersRefresh)
            {
                KSCVesselMarkers.fetch?.RefreshMarkers();
                TriggerMarkersRefresh = false;
            }
        }
    }
}

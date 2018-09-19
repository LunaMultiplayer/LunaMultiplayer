using LmpClient.Base;
using LmpClient.Events;

namespace LmpClient.Systems.KscScene
{
    /// <summary>
    /// This class controls what happens when we are in KSC screen
    /// </summary>
    public class KscSceneSystem : System<KscSceneSystem>
    {
        private static KscSceneEvents KscSceneEvents { get; } = new KscSceneEvents();

        #region Base overrides

        public override string SystemName { get; } = nameof(KscSceneSystem);
        
        protected override void OnEnabled()
        {
            LockEvent.onLockAcquireUnityThread.Add(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Add(KscSceneEvents.OnLockRelease);
            GameEvents.onGameSceneLoadRequested.Add(KscSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Add(KscSceneEvents.LevelLoaded);
        }

        protected override void OnDisabled()
        {
            LockEvent.onLockAcquireUnityThread.Remove(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Remove(KscSceneEvents.OnLockRelease);
            GameEvents.onGameSceneLoadRequested.Remove(KscSceneEvents.OnSceneRequested);
            GameEvents.onLevelWasLoadedGUIReady.Remove(KscSceneEvents.LevelLoaded);
        }

        #endregion
    }
}

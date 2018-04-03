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
        
        protected override void OnEnabled()
        {
            LockEvent.onLockAcquireUnityThread.Add(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Add(KscSceneEvents.OnLockRelease);
        }

        protected override void OnDisabled()
        {
            LockEvent.onLockAcquireUnityThread.Remove(KscSceneEvents.OnLockAcquire);
            LockEvent.onLockReleaseUnityThread.Remove(KscSceneEvents.OnLockRelease);
        }

        #endregion
    }
}

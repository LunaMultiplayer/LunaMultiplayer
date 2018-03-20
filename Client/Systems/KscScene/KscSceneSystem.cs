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

        protected override void OnEnabled()
        {
            LockSystem.Singleton.RegisterAcquireHook(KscSceneEvents.OnLockAcquire);
            LockSystem.Singleton.RegisterReleaseHook(KscSceneEvents.OnLockRelease);
        }

        protected override void OnDisabled()
        {
            LockSystem.Singleton.UnregisterAcquireHook(KscSceneEvents.OnLockAcquire);
            LockSystem.Singleton.UnregisterReleaseHook(KscSceneEvents.OnLockRelease);
        }

        #endregion
    }
}

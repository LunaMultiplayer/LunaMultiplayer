using LunaCommon.Locks;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class LockEvent
    {
        public static EventData<LockDefinition> onLockAcquire { get; } = new EventData<LockDefinition>("onLockAcquire");
        public static EventData<LockDefinition> onLockRelease { get; } = new EventData<LockDefinition>("onLockRelease");
        public static EventData<LockDefinition> onLockAcquireUnityThread { get; } = new EventData<LockDefinition>("onLockAcquireUnityThread");
        public static EventData<LockDefinition> onLockReleaseUnityThread { get; } = new EventData<LockDefinition>("onLockReleaseUnityThread");
    }
}

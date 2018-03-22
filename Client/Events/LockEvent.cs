using LunaCommon.Locks;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class LockEvent
    {
        public static EventData<LockDefinition> onLockAcquire = new EventData<LockDefinition>("onLockAcquire");
        public static EventData<LockDefinition> onLockRelease = new EventData<LockDefinition>("onLockRelease");
        public static EventData<LockDefinition> onLockAcquireUnityThread = new EventData<LockDefinition>("onLockAcquireUnityThread");
        public static EventData<LockDefinition> onLockReleaseUnityThread = new EventData<LockDefinition>("onLockReleaseUnityThread");
    }
}

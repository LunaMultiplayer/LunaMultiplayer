using LunaCommon.Locks;
// ReSharper disable All
#pragma warning disable IDE1006

namespace LunaClient.Events
{
    public static class LockEvent
    {
        public static EventData<LockDefinition> onLockAcquire = new EventData<LockDefinition>("onLockAcquire");
        public static EventData<LockDefinition> onLockRelease = new EventData<LockDefinition>("onLockRelease");
    }
}

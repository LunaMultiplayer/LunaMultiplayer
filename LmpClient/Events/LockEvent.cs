using LmpClient.Events.Base;
using LmpCommon.Locks;

// ReSharper disable All
#pragma warning disable IDE1006

namespace LmpClient.Events
{
    public class LockEvent : LmpBaseEvent
    {
        public static EventData<LockDefinition> onLockAcquire;
        public static EventData<LockDefinition> onLockRelease;
        public static EventData<LockDefinition> onLockAcquireUnityThread;
        public static EventData<LockDefinition> onLockReleaseUnityThread;
    }
}

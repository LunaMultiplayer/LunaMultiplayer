using LunaCommon.Locks;

namespace LunaClient.Systems.Lock
{
    public delegate void AcquireEvent(LockDefinition lockDefinition, bool lockResult);

    public delegate void ReleaseEvent(LockDefinition lockDefinition);
}
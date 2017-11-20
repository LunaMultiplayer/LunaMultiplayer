using LunaCommon.Locks;

namespace LunaClient.Systems.Lock
{
    public delegate void AcquireEvent(LockDefinition lockDefinition);

    public delegate void ReleaseEvent(LockDefinition lockDefinition);
}
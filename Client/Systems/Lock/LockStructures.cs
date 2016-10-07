namespace LunaClient.Systems.Lock
{
    public delegate void AcquireEvent(string playerName, string lockName, bool lockResult);

    public delegate void ReleaseEvent(string playerName, string lockName);
}
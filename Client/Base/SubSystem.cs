using LunaClient.Base.Interface;

namespace LunaClient.Base
{
    /// <summary>
    /// This class should be implemented by subsistems of a system (message senders, message handlers, etc)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SubSystem<T> : SystemBase
        where T : class, ISystem, new()
    {
        protected static T System => System<T>.Singleton;
    }
}
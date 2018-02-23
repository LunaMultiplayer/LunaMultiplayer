using LunaClient.Base.Interface;
using LunaClient.Systems;

namespace LunaClient.Base
{
    /// <summary>
    /// This class should be implemented by subsistems of a system (message senders, message handlers, etc)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SubSystem<T> : SystemBase
        where T : class, ISystem, new()
    {
        /// <summary>
        /// Reference to the main system where this subsystem belongs
        /// </summary>
        protected static T System => SystemsContainer.Get<T>();
    }
}
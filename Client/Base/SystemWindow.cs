using LunaClient.Base.Interface;
using LunaClient.Systems;

namespace LunaClient.Base
{
    /// <summary>
    /// Base class for systems that also implement a window (chat for example)
    /// </summary>
    public abstract class SystemWindow<T, TS> : Window<T>
        where T : class, IWindow, new()
        where TS : class, ISystem
    {
        /// <summary>
        /// Reference to the main system where this system window belongs
        /// </summary>
        public TS System => SystemsContainer.Get<TS>();
    }
}
using LmpClient.Base.Interface;
using System.Reflection;

namespace LmpClient.Base
{
    /// <inheritdoc />
    /// <summary>
    /// This class should be implemented by subsystems of a system (message senders, message handlers, etc)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SubSystem<T> : SystemBase
        where T : class, ISystem, new()
    {
        private static T _system;

        /// <summary>
        /// Reference to the main system where this subsystem belongs
        /// </summary>
        protected static T System
        {
            get
            {
                if (_system == null)
                    _system = typeof(T).GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null, null) as T;

                return _system;
            }
        }
    }
}
using LmpClient.Base.Interface;
using System.Reflection;

namespace LmpClient.Base
{
    /// <summary>
    /// Base class for systems that also implement a window (chat for example)
    /// </summary>
    public abstract class SystemWindow<T, TS> : Window<T>
        where T : class, IWindow, new()
        where TS : class, ISystem
    {
        private bool _display;

        public override bool Display
        {
            get => _display && System.Enabled;
            set => _display = value;
        }


        private static TS _system;

        /// <summary>
        /// Reference to the main system where this system window belongs
        /// </summary>
        protected static TS System
        {
            get
            {
                if (_system == null)
                    _system = typeof(TS).GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)?.GetValue(null, null) as TS;

                return _system;
            }
        }
    }
}
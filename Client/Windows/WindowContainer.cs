using LunaClient.Base.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Windows
{
    public class WindowsContainer
    {
        private static readonly ConcurrentDictionary<Type, IWindow> Windows = new ConcurrentDictionary<Type, IWindow>();

        public static T Get<T>() where T : class, IWindow
        {
            if (!Windows.TryGetValue(typeof(T), out var window))
            {
                window = Activator.CreateInstance<T>();
                Windows.TryAdd(typeof(T), window);
            }

            return window as T;
        }

        public static IWindow Get(Type type)
        {
            if (!typeof(IWindow).IsAssignableFrom(type))
                return null;

            if (!Windows.TryGetValue(type, out var window))
            {
                window = Activator.CreateInstance(type) as IWindow;
                Windows.TryAdd(type, window);
            }

            return window;
        }
    }
}

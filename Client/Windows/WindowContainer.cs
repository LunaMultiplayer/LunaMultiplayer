using LunaClient.Base.Interface;
using System;
using System.Collections.Generic;

namespace LunaClient.Windows
{
    public class WindowsContainer
    {

        private static readonly Dictionary<Type, IWindow> Windows = new Dictionary<Type, IWindow>();

        public static T Get<T>() where T : class, IWindow
        {
            var type = typeof(T);
            Windows.TryGetValue(type, out var instance);

            if (instance == null)
            {
                instance = Activator.CreateInstance<T>();
                Windows.Add(type, instance);
            }

            return instance as T;
        }
    }
}

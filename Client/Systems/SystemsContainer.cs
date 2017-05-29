using LunaClient.Base.Interface;
using System;
using System.Collections.Generic;

namespace LunaClient.Systems
{
    public class SystemsContainer
    {

        private static readonly Dictionary<Type, ISystem> Systems = new Dictionary<Type, ISystem>();

        public static T Get<T>() where T : class, ISystem
        {
            var type = typeof(T);
            Systems.TryGetValue(type, out var instance);

            if (instance == null)
            {
                instance = Activator.CreateInstance<T>();
                Systems.Add(type, instance);
            }

            return instance as T;
        }
    }
}

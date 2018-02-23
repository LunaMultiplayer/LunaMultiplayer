using LunaClient.Base.Interface;
using System;
using System.Collections.Concurrent;

namespace LunaClient.Systems
{
    public class SystemsContainer
    {
        private static readonly ConcurrentDictionary<Type, ISystem> Systems = new ConcurrentDictionary<Type, ISystem>();

        public static T Get<T>() where T : class, ISystem
        {
            if (!Systems.TryGetValue(typeof(T), out var system))
            {
                system = Activator.CreateInstance<T>();
                Systems.TryAdd(typeof(T), system);
            }

            return system as T;
        }

        public static ISystem Get(Type type)
        {
            if (!typeof(ISystem).IsAssignableFrom(type))
                return null;

            if (!Systems.TryGetValue(type, out var system))
            {
                system = Activator.CreateInstance(type) as ISystem;
                Systems.TryAdd(type, system);
            }

            return system;
        }
    }
}

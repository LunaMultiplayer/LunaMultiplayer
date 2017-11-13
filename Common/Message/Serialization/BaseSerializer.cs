using FastMember;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace LunaCommon.Message.Serialization
{
    /// <summary>
    /// Class to represent a PropertyInfo with only the information we need to reduce the memory usage
    /// </summary>
    internal class ReducedPropertyInfo
    {
        internal string Name { get; set; }
        internal Type PropertyType { get; set; }
        internal bool CanWrite { get; set; }

        internal ReducedPropertyInfo(string name, Type propertyType, bool canWrite)
        {
            Name = name;
            PropertyType = propertyType;
            CanWrite = canWrite;
        }
    }

    internal static class BaseSerializer
    {
        /// <summary>
        ///     Dictionary of properties based on a MessageData type
        /// </summary>
        private static readonly ConcurrentDictionary<Type, IEnumerable<ReducedPropertyInfo>> PropertyDictionary =
            new ConcurrentDictionary<Type, IEnumerable<ReducedPropertyInfo>>();

        /// <summary>
        ///     Dictionary of accessors based on a MessageData type
        /// </summary>
        private static readonly ConcurrentDictionary<Type, TypeAccessor> AccessorDictionary =
            new ConcurrentDictionary<Type, TypeAccessor>();

        /// <summary>
        ///     Encoder for serialization/deserialization of strings
        /// </summary>
        internal static readonly UnicodeEncoding Encoder = new UnicodeEncoding();

        /// <summary>
        ///     Method to retrieve the properties of a MessageData. We catch them as reflection is very slow
        /// </summary>
        /// <returns>List of properties</returns>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        internal static List<ReducedPropertyInfo> GetCachedProperties(Type type)
        {
            if (!PropertyDictionary.TryGetValue(type, out var properties))
            {
                //We never store the Version or the receive time property of the MessageData class
                //Order them with the get only properties on top to increase speed and then by alphabetical order
                properties = type.GetProperties().Where(p => p.Name != "Version" && p.Name != "ReceiveTime")
                    .Select(p => new ReducedPropertyInfo(p.Name, p.PropertyType, p.CanWrite))
                    .OrderBy(v => v.CanWrite)
                    .ThenBy(p => p.Name);

                PropertyDictionary.TryAdd(type, properties);
            }
            
            return properties.ToList();
        }

        /// <summary>
        ///     Method to retrieve the accessor
        /// </summary>
        /// <returns>Accessor</returns>
        internal static TypeAccessor GetCachedTypeAccessor(Type type)
        {
            return AccessorDictionary.GetOrAdd(type, TypeAccessor.Create(type)); ;
        }
    }
}
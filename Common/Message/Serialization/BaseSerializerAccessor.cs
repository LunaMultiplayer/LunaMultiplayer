using System;
using System.Collections.Generic;
using FastMember;

namespace DarkMultiPlayerCommon.Message.Serialization
{
    internal static class BaseSerializerAccessor
    {
        private static readonly object SyncRootAccessorDict = new object();

        /// <summary>
        ///     Dictionary of accessors based on a MessageData type
        /// </summary>
        private static readonly Dictionary<Type, CustomTypeAccessor> AccessorDictionary =
            new Dictionary<Type, CustomTypeAccessor>();

        /// <summary>
        ///     Method to retrieve the accessor
        /// </summary>
        /// <returns>Accessor</returns>
        internal static TypeAccessor GetCachedTypeAccessor(Type type)
        {
            lock (SyncRootAccessorDict)
            {
                CustomTypeAccessor accessor;
                if (!AccessorDictionary.TryGetValue(type, out accessor))
                {
                    accessor = (CustomTypeAccessor)TypeAccessor.Create(type);
                    AccessorDictionary.Add(type, accessor);
                }

                return accessor;
            }
        }
    }
}

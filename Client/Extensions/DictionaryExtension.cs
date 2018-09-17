using System;
using System.Collections.Generic;

namespace LunaClient.Extensions
{
    public static class DictionaryExtension
    {
        public static TU GetOrAdd<T, TU>(this Dictionary<T, TU> dict, T key, Func<TU> create)
        {
            if (!dict.TryGetValue(key, out var val))
            {
                val = create();
                dict[key] = val;
            }

            return val;
        }
    }
}

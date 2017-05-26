using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaClient.Utilities
{
    public class CommonUtil
    {
        public static double Now => (double)DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// Combine the paths specified as .net 3.5 doesn't give you a good method
        /// </summary>
        public static string CombinePaths(params string[] paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            return paths.Aggregate(Path.Combine);
        }

        /// <summary>
        /// Compare two ienumerables and return if they are the same or not IGNORING the order
        /// </summary>
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var list1Enu = list1 as T[] ?? list1.ToArray();
            var list2Enu = list2 as T[] ?? list2.ToArray();
            if (list1Enu.Length != list2Enu.Length)
            {
                return false;
            }

            var cnt = new Dictionary<T, int>();
            foreach (var s in list1Enu)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else
                {
                    cnt.Add(s, 1);
                }
            }
            foreach (var s in list2Enu)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else
                {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }
    }
}

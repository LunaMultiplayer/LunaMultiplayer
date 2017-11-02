using System;
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
    }
}

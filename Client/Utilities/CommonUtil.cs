using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LunaClient.Utilities
{
    public class CommonUtil
    {
        public static double Now => (double)DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond;

        private static string _debugPort;
        public static string DebugPort => _debugPort ?? (_debugPort = GetDebugPort());

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
        /// This returns the port that you must attach to from Visual studio.
        /// Only useful when you have several KSPinstances and you don't know wich one to attach
        /// </summary>
        private static string GetDebugPort()
        {
            var outputLogFile = CombinePaths(Client.KspPath, "KSP_x64_Data", "output_log.txt");

            var regex = new Regex(@"0\.0\.0\.0:(\d+)");

            using (var stream = File.Open(outputLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var match = regex.Match(line);
                    if (match.Success && match.Groups.Count > 1)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }

            return null;
        }
    }
}

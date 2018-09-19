using LmpCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LmpClient.Utilities
{
    public class CommonUtil
    {
        private static readonly Random Rnd = new Random();

        private static string _debugPort;
        public static string DebugPort => _debugPort ?? (_debugPort = GetDebugPort());

        public static string OutputLogFilePath = CombinePaths(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..", "LocalLow", "Squad", "Kerbal Space Program", "output_log.txt");

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
        /// Only useful when you have several KSPinstances and you don't know which one to attach
        /// </summary>
        private static string GetDebugPort()
        {
            if (!Common.PlatformIsWindows()) return "0";

            if (!File.Exists(OutputLogFilePath)) return "0";

            var regex = new Regex(@"address=0\.0\.0\.0:(\d+)");

            using (var stream = File.Open(OutputLogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

        /// <summary>
        /// Compare two ienumerables and return if they are the same or not IGNORING the order
        /// </summary>
        public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            var deletedItems = list1.Except(list2).Any();
            var newItems = list2.Except(list1).Any();
            return !newItems && !deletedItems;
        }

        /// <summary>
        /// Allocates 20mb in the mono heap
        /// </summary>
        public static void Reserve20Mb()
        {
            var bytes = new byte[20971520];
            Rnd.NextBytes(bytes);
        }
    }
}

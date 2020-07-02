using LmpCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LmpClient.Utilities
{
    public class CommonUtil
    {
        private static readonly Random Rnd = new Random();

        private static string _processId;
        public static string ProcessId => _processId ?? (_processId = Process.GetCurrentProcess().Id.ToString());

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
        /// Compare two IEnumerable and return if they are the same or not IGNORING the order
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

using LunaCommon.Time;
using Server.Context;
using Server.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.System
{
    public class WarpSystem
    {
        private static string SubspaceFile { get; } = Path.Combine(ServerContext.UniverseDirectory, "Subspace.txt");

        public static void Reset()
        {
            WarpContext.Subspaces.Clear();
            LoadSavedSubspace();
        }

        public static void SaveSubspacesToFile()
        {
            var subspaces = WarpContext.Subspaces.ToArray();

            var content = $"#Incorrectly editing this file will cause weirdness. If there is any errors, the universe time will be reset.{Environment.NewLine}";
            content += $"#This file can only be edited if the server is stopped.{Environment.NewLine}";
            content += $"#Each variable is defined as: subspaceId:server_time_difference_in_seconds.{Environment.NewLine}";
            content += $"#It must always contain at least 1 subspace which will be the most advanced in the future{Environment.NewLine}";

            content = subspaces.Aggregate(content, (current, subspace) => current + $"{subspace.Key}:{subspace.Value}{Environment.NewLine}");

            FileHandler.WriteToFile(SubspaceFile, content);
        }

        public static void RemoveSubspace(int oldSubspace)
        {
            //If there's only 1 subspace do not remove it!
            if (WarpContext.Subspaces.Count == 1)
                return;

            //We are in the latest subspace and we NEVER remove it!
            if (oldSubspace == WarpContext.LatestSubspace)
            {
                //Get old subspaces that are empty (except this one) and remove them (cleanup)
                var emptySubspaces = GetEmptySubspaces().Where(s => s != oldSubspace);
                foreach (var emptySubspace in emptySubspaces)
                {
                    WarpContext.Subspaces.TryRemove(emptySubspace, out var _);
                }
            }
            else
            {
                WarpContext.Subspaces.TryRemove(oldSubspace, out var _);
            }
        }

        #region Private methods
        
        private static void LoadSavedSubspace()
        {
            if (FileHandler.FileExists(SubspaceFile))
            {
                var subspaceLines = GetSubspaceLinesFromFile();
                foreach (var line in subspaceLines)
                {
                    WarpContext.Subspaces.TryAdd(line.Key, line.Value);
                }

                WarpContext.NextSubspaceId = WarpContext.Subspaces.Any() ? WarpContext.Subspaces.Max(s => s.Key) + 1 : 1;
            }
            else
            {
                LunaLog.Debug("Creating new subspace dictionary");
                WarpContext.Subspaces.TryAdd(0, 0);
                WarpContext.NextSubspaceId = 1;
            }
        }

        private static IEnumerable<KeyValuePair<int, double>> GetSubspaceLinesFromFile()
        {
            var subspaceLines = FileHandler.ReadFileLines(SubspaceFile)
                .Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
                .Select(s => new KeyValuePair<int, double>(int.Parse(s.Split(':')[0]), double.Parse(s.Split(':')[1])));

            return subspaceLines;
        }

        /// <summary>
        /// Returns the time difference of the given subspace against the server time in ticks
        /// </summary>
        public static long GetSubspaceTimeDifference(int subspace)
        {
            return WarpContext.Subspaces.ContainsKey(subspace) ? (long)TimeUtil.SecondsToTicks(WarpContext.Subspaces[subspace]) : 0;
        }

        /// <summary>
        /// Returns the time in ticks at the given subspace
        /// </summary>
        public static long GetSubspaceTime(int subspace)
        {
            return LunaTime.UtcNow.Ticks + GetSubspaceTimeDifference(subspace);
        }

        /// <summary>
        /// Returns the subspaces that runs in an earlier time (this means the subspaces that have a LOWER time difference)
        /// </summary>
        public static int[] GetPastSubspaces(int subspace)
        {
            if (!WarpContext.Subspaces.ContainsKey(subspace))
                return new int[0];

            return WarpContext.Subspaces.Where(s => s.Key != subspace && WarpContext.Subspaces.TryGetValue(subspace, out var time) && s.Value < time).Select(s => s.Key).ToArray();
        }

        /// <summary>
        /// Returns the subspaces that runs in an future time (this means the subspaces that have a HIGHER time difference)
        /// </summary>
        public static int[] GetFutureSubspaces(int subspace)
        {
            return WarpContext.Subspaces.Where(s => s.Key != subspace && WarpContext.Subspaces.TryGetValue(subspace, out var time) && s.Value > time).Select(s => s.Key).ToArray();
        }

        /// <summary>
        /// Returns the empty subspaces. Caution as here the latest subspace can be included!
        /// </summary>
        public static int[] GetEmptySubspaces()
        {
            return WarpContext.Subspaces.ToArray().Where(s => !ServerContext.Clients.Any(c => c.Value.Subspace == s.Key)).Select(s => s.Key).ToArray();
        }

        #endregion

    }
}

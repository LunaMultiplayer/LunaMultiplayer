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
            content += $"#It must always contain at least 1 subspace wich will be the most advanced in the future{Environment.NewLine}";

            content = subspaces.Aggregate(content, (current, subspace) => current + $"{subspace.Key}:{subspace.Value}{Environment.NewLine}");

            FileHandler.WriteToFile(SubspaceFile, content);
        }

        public static void RemoveSubspace(int oldSubspace)
        {
            //We never remove the latest subspace or the only subspace from the server!
            if (WarpContext.Subspaces.Count == 1 || oldSubspace == WarpContext.LatestSubspace)
                return;

            WarpContext.Subspaces.TryRemove(oldSubspace, out var _);
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
        /// Returns the current time in ticks at the given subspace
        /// </summary>
        public static long GetCurrentSubspaceTime(int subspace)
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

        #endregion

    }
}
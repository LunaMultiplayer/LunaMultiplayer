using LMP.Server.Context;
using LMP.Server.Log;
using LunaCommon.Time;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LMP.Server.System
{
    public class WarpSystem
    {
        private static string SubspaceFile { get; } = Path.Combine(ServerContext.UniverseDirectory, "Subspace.txt");

        public static void Reset()
        {
            WarpContext.Subspaces.Clear();
            LoadSavedSubspace();
        }

        public static void SaveSubspace(int subspaceId, double subspaceTime)
        {
            FileHandler.AppendToFile(SubspaceFile, $"{subspaceId}:{subspaceTime}{Environment.NewLine}");
        }

        public static void RemoveSubspace(int oldSubspace)
        {
            //We never remove the latest subspace or the only subspace from the server!
            if (WarpContext.Subspaces.Count == 1 || oldSubspace == WarpContext.LatestSubspace)
                return;

            if (WarpContext.Subspaces.TryRemove(oldSubspace, out var _))
            {
                var allLinesExceptTheDeleted = string.Join(Environment.NewLine, GetSubspaceLinesFromFile()
                    .Where(s => s.Key != oldSubspace)
                    .Select(s => $"{s.Key}:{s.Value}"));

                //Calling WriteHeaderToSubspaceFile will remove what's already in the file...
                WriteHeaderToSubspaceFile();
                FileHandler.AppendToFile(SubspaceFile, allLinesExceptTheDeleted);
            }
        }

        #region Private methods

        private static void WriteHeaderToSubspaceFile()
        {
            var content = $"#Incorrectly editing this file will cause weirdness. If there is any errors, the universe time will be reset.{Environment.NewLine}";
            content += $"#This file can only be edited if the server is stopped.{Environment.NewLine}";
            content += $"#Each variable is defined as: subspaceId:server_time_difference_in_seconds.{Environment.NewLine}";
            content += $"#It must always contain at least 1 subspace wich will be the most advanced in the future{Environment.NewLine}";

            FileHandler.WriteToFile(SubspaceFile, content);
        }

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
                LunaLog.Debug("Creating new Subspace lock file");
                WriteHeaderToSubspaceFile();
                WarpContext.Subspaces.TryAdd(0, 0);
                SaveSubspace(0, 0);
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
            return WarpContext.Subspaces.ContainsKey(subspace) ? TimeSpan.FromSeconds(WarpContext.Subspaces[subspace]).Ticks : 0;
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
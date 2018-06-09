using LunaCommon.Time;
using Server.Context;
using Server.Events;
using Server.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Server.System
{
    public static class WarpSystem
    {
        static WarpSystem() => ExitEvent.ServerClosing += SaveLatestSubspaceToFile;

        private static string SubspaceFile { get; } = Path.Combine(ServerContext.UniverseDirectory, "Subspace.txt");

        public static void Reset()
        {
            WarpContext.Subspaces.Clear();
            LoadSavedSubspace();
        }

        public static void SaveLatestSubspaceToFile()
        {
            var content = $"#Incorrectly editing this file will cause weirdness. If there is any errors, the universe time will be reset.{Environment.NewLine}";
            content += $"#This file can only be edited if the server is stopped.{Environment.NewLine}";
            content += $"#It must always contain ONLY 1 subspace which will be the most advanced in the future{Environment.NewLine}";
            content += $"#The value is defined as: subspaceId:server_time_difference_in_seconds.{Environment.NewLine}";

            content += $"{WarpContext.LatestSubspace}";

            FileHandler.WriteToFile(SubspaceFile, content);
        }

        public static bool RemoveSubspace(int subspaceToRemove)
        {
            //Do not remove the subspace if there are clients there
            if (ServerContext.Clients.Any(c => c.Value.Subspace == subspaceToRemove))
                return false;

            //If there's only 1 subspace do not remove it!
            if (WarpContext.Subspaces.Count == 1)
                return false;

            //We are in the latest subspace and we NEVER remove it!
            if (subspaceToRemove == WarpContext.LatestSubspace.Id)
                return false;

            LunaLog.Debug($"Removing abandoned subspace '{subspaceToRemove}'");
            WarpContext.Subspaces.TryRemove(subspaceToRemove, out var _);
            return true;
        }

        #region Private methods
        
        private static void LoadSavedSubspace()
        {
            if (FileHandler.FileExists(SubspaceFile))
            {
                var latestStoredSubspace = GetLatestSubspaceLineFromFile();
                WarpContext.Subspaces.TryAdd(latestStoredSubspace.Key, new Subspace(latestStoredSubspace.Key, latestStoredSubspace.Value));
                WarpContext.NextSubspaceId = WarpContext.Subspaces.Max(s => s.Key) + 1;
            }
            else
            {
                LunaLog.Debug("Creating new subspace file");
                WarpContext.Subspaces.TryAdd(0, new Subspace(0));
                WarpContext.NextSubspaceId = 1;
            }
        }

        private static KeyValuePair<int, double> GetLatestSubspaceLineFromFile()
        {
            var subspaceLines = FileHandler.ReadFileLines(SubspaceFile)
                .Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#"))
                .Select(s => new KeyValuePair<int, double>(int.Parse(s.Split(':')[0]), double.Parse(s.Split(':')[1])))
                .ToArray();

            if (subspaceLines.Length == 0)
            {
                LunaLog.Error("Incorrect Subspace.txt file!");
                return new KeyValuePair<int, double>(0,0);
            }
            
            //TODO: Retrocompatibility - Remove next 2 lines 2/3 months after 1/july/2018
            if (subspaceLines.Length > 1) 
                return subspaceLines.OrderByDescending(s => s.Value).First();

            //TODO: Uncomment this 2/3 months after 1/july/2018
            //if (SubspaceFile.Length > 1)
            //{
            //    LunaLog.Error("Incorrect Subspace.txt file!");
            //    return subspaceLines.OrderByDescending(s => s.Value).First();
            //}

            return subspaceLines.First();
        }

        /// <summary>
        /// Returns the time difference of the given subspace against the server time in ticks
        /// </summary>
        public static long GetSubspaceTimeDifference(int subspace)
        {
            return WarpContext.Subspaces.ContainsKey(subspace) ? TimeUtil.SecondsToTicks(WarpContext.Subspaces[subspace].Time) : 0;
        }

        /// <summary>
        /// Returns the time in ticks at the given subspace
        /// </summary>
        public static long GetSubspaceTime(int subspace)
        {
            return LunaNetworkTime.UtcNow.Ticks + GetSubspaceTimeDifference(subspace);
        }

        /// <summary>
        /// Returns the subspaces that runs in an earlier time (this means the subspaces that have a LOWER time difference)
        /// </summary>
        public static int[] GetPastSubspaces(int subspace)
        {
            if (!WarpContext.Subspaces.ContainsKey(subspace))
                return new int[0];

            return WarpContext.Subspaces.Values.Where(s => s.Id != subspace && WarpContext.Subspaces.TryGetValue(subspace, out var anotherSubspace) && s.Time < anotherSubspace.Time)
                .Select(s => s.Id).ToArray();
        }

        /// <summary>
        /// Returns the subspaces that runs in an future time (this means the subspaces that have a HIGHER time difference)
        /// </summary>
        public static int[] GetFutureSubspaces(int subspace)
        {
            return WarpContext.Subspaces.Values.Where(s => s.Id != subspace && WarpContext.Subspaces.TryGetValue(subspace, out var anotherSubspace) && s.Time > anotherSubspace.Time)
                .Select(s => s.Id).ToArray();
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

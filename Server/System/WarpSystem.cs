using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaServer.Context;
using LunaServer.Log;

namespace LunaServer.System
{
    public class WarpSystem
    {
        private static string SubspaceFile { get; } = Path.Combine(ServerContext.UniverseDirectory, "Subspace.txt");
        
        public static void DisconnectPlayer(string playerName)
        {
            lock (WarpContext.ListLock)
            {
                if (WarpContext.IgnoreList != null)
                    if (WarpContext.IgnoreList.Contains(playerName))
                        WarpContext.IgnoreList.Remove(playerName);
            }
        }

        public static void Reset()
        {
            WarpContext.Subspaces.Clear();
            WarpContext.OfflinePlayerSubspaces.Clear();
            lock (WarpContext.ListLock)
            {
                WarpContext.IgnoreList = null;
            }
            LoadSavedSubspace();
        }
        
        public static void SaveSubspace(int subspaceId, double subspaceTime)
        {
            FileHandler.AppendToFile(SubspaceFile, subspaceId + ":" + subspaceTime + Environment.NewLine);
        }
        
        public static void RemoveSubspace(int oldSubspace)
        {
            double time;
            WarpContext.Subspaces.TryRemove(0, out time);

            var allLinesExceptTheDeleted = string.Join(Environment.NewLine, GetSubspaceLinesFromFile()
                .Where(s => s.Key != oldSubspace)
                .Select(s => s.Key + ":" + s.Value));

            WriteHeaderToSubspaceFile();
            FileHandler.AppendToFile(SubspaceFile, allLinesExceptTheDeleted);
        }

        #region Private methods

        private static void WriteHeaderToSubspaceFile()
        {
            var content = "#Incorrectly editing this file will cause weirdness. If there is any errors, " +
                          "the universe time will be reset." + Environment.NewLine;
            content += "#This file can only be edited if the server is stopped." + Environment.NewLine;
            content += "#Each variable is defined as: subspaceId:subspace time." + Environment.NewLine;

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

                WarpContext.NextSubspaceId = WarpContext.Subspaces.Any() ? WarpContext.Subspaces.Max(s=> s.Key) + 1: 1;
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
                .Select(l => l.Trim()).Where(l => !l.StartsWith("#") && !string.IsNullOrEmpty(l))
                .Select(s => new KeyValuePair<int, double>(int.Parse(s.Split(':')[0]), double.Parse(s.Split(':')[1])));

            return subspaceLines;
        }

        #endregion
    }
}
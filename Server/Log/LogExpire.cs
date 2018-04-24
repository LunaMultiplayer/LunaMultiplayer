using LunaCommon.Time;
using Server.Context;
using Server.Settings.Structures;
using Server.System;
using System;
using System.IO;

namespace Server.Log
{
    public class LogExpire
    {
        //Do not use filehandler on log as it's on it's own way
        private static string LogDirectory => Path.Combine(ServerContext.UniverseDirectory, LunaLog.LogFolder);

        public static void ExpireLogs()
        {
            //Check if the ExpireLogs setting is enabled and directory exists
            if (LogSettings.SettingsStore.ExpireLogs > 0 && FileHandler.FolderExists(LogDirectory))
                foreach (var logFile in FileHandler.GetFilesInPath(LogDirectory))
                    RemoveExpiredLog(logFile);
        }

        private static void RemoveExpiredLog(string logFile)
        {
            //If the file is older than a day, delete it
            if (File.GetCreationTime(logFile).AddDays(LogSettings.SettingsStore.ExpireLogs) < LunaTime.Now)
            {
                LunaLog.Debug($"Deleting saved log '{logFile}', reason: Expired!");
                try
                {
                    FileHandler.FileDelete(logFile);
                }
                catch (Exception e)
                {
                    LunaLog.Error($"Exception while trying to delete '{logFile}'!, Exception: {e.Message}");
                }
            }
        }
    }
}

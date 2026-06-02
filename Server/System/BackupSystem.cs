using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;
using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;

namespace Server.System
{
    public class BackupSystem
    {
        //Subscribe to the exit event so a backup is performed when closing the server
        static BackupSystem() => ExitEvent.ServerClosing += RunSave;

        private static readonly object LockObj = new object();

        public static Task PerformBackupsAsync(CancellationToken token)
        {
            return Task.WhenAll(
                RunSavesAsync(token),
                RunBackupsAsync(token)
            );
        }

        private static async Task RunSavesAsync(CancellationToken token)
        {
            while (ServerContext.ServerRunning)
            {
                if (ServerContext.PlayerCount > 0)
                    RunSave();

                try
                {
                    await Task.Delay(IntervalSettings.SettingsStore.SaveIntervalMs, token);
                }
                catch (TaskCanceledException) { break; }
            }
        }

        private static async Task RunBackupsAsync(CancellationToken token)
        {
            if (IntervalSettings.SettingsStore.BackupIntervalMs == 0 || GeneralSettings.SettingsStore.MaxBackups == 0) {
                LunaLog.Normal("Backups are off");
                return;
            }

            while (ServerContext.ServerRunning)
            {
                RunBackup();

                try
                {
                    await Task.Delay(IntervalSettings.SettingsStore.BackupIntervalMs, token);
                }
                catch (TaskCanceledException) { break; }
            }
        }

        public static void RunSave()
        {
            lock (LockObj)
            {
                LunaLog.Debug("Performing save...");
                VesselStoreSystem.BackupVessels();
                WarpSystem.BackupSubspaces();
                TimeSystem.BackupStartTime();
                ScenarioStoreSystem.BackupScenarios();
                LunaLog.Debug("Saving done");
            }
        }

        private static void RemoveBackupsAboveLimit()
        {
            var backups = Directory.EnumerateFiles(ServerContext.BackupDirectory, "backup_*.zip")
                .Select(f => new
                {
                    path = f,
                    name = Path.GetFileNameWithoutExtension(f)
                })
                .Select(f =>
                {
                    var backupDateString = f.name.Substring("backup_".Length);
                    if (DateTime.TryParseExact(backupDateString, "yyyy-MM-dd_HH-mm-ss",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                        return new { f.path, Date = (DateTime?)dt };

                    return new { f.path, Date = (DateTime?)null };
                })
                .Where(f => f.Date.HasValue)
                .OrderByDescending(f => f.Date)
                .ToList();

            while (backups.Count >= GeneralSettings.SettingsStore.MaxBackups)
            {
                LunaLog.Debug($"Deleting old backup from: {backups.Last().Date}");
                File.Delete(backups.Last().path);
                backups.RemoveAt(backups.Count - 1);
            }
        }

        public static void RunBackup()
        {
            LunaLog.Debug("Performing backup...");

            if (!Path.Exists(ServerContext.BackupDirectory))
                Directory.CreateDirectory(ServerContext.BackupDirectory);

            RemoveBackupsAboveLimit();
            RunSave();

            var tempUniversePath = Path.Combine(ServerContext.BackupDirectory, "temp_backup");
            if (Directory.Exists(tempUniversePath))
                Directory.Delete(tempUniversePath, recursive: true);

            foreach (var dirPath in Directory.EnumerateDirectories(ServerContext.UniverseDirectory, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(ServerContext.UniverseDirectory, tempUniversePath));

            foreach (var filePath in Directory.EnumerateFiles(ServerContext.UniverseDirectory, "*", SearchOption.AllDirectories))
                File.Copy(filePath, filePath.Replace(ServerContext.UniverseDirectory, tempUniversePath), overwrite: true);

            ZipFile.CreateFromDirectory(tempUniversePath, Path.Combine(ServerContext.BackupDirectory, $"backup_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.zip"));
            Directory.Delete(tempUniversePath, recursive: true);

            LunaLog.Debug("Backups done");
        }
    }
}

using Server.Context;
using Server.Events;
using Server.Log;
using Server.Settings.Structures;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    public class BackupSystem
    {
        //Subscribe to the exit event so a backup is performed when closing the server
        static BackupSystem() => ExitEvent.ServerClosing += RunSave;

        private static readonly object LockObj = new object();

        public static async Task PerformBackupsAsync(CancellationToken token)
        {
            while (ServerContext.ServerRunning)
            {
                if (ServerContext.PlayerCount > 0)
                {
                    RunSave();
                }

                try
                {
                    await Task.Delay(IntervalSettings.SettingsStore.SaveIntervalMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
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

        public static void RunBackup()
        {
            lock (LockObj)
            {
                LunaLog.Debug("Performing backup...");
                LunaLog.Debug("Backups done");
            }
        }
    }
}

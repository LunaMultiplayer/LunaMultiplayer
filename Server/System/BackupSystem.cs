using Server.Context;
using Server.Log;
using Server.Settings;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    public class BackupSystem
    {
        public static async void PerformBackups(CancellationToken token)
        {
            LunaLog.Debug("Performing backups to files...");
            while (ServerContext.ServerRunning)
            {
                VesselStoreSystem.BackupVessels();
                try
                {
                    await Task.Delay(GeneralSettings.SettingsStore.VesselsBackupIntervalMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            //Do a last backup before quitting
            VesselStoreSystem.BackupVessels();
        }
    }
}

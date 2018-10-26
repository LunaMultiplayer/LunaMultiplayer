using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using System.Threading;
using System.Threading.Tasks;

namespace Server.System
{
    public class BackupSystem
    {
        public static async void PerformBackups(CancellationToken token)
        {
            while (ServerContext.ServerRunning)
            {
                if (ServerContext.PlayerCount > 0)
                {
                    LunaLog.Debug("Performing backups...");
                    VesselStoreSystem.BackupVessels();
                    WarpSystem.SaveLatestSubspaceToFile();
                    ScenarioStoreSystem.BackupScenarios();
                }
                else
                {
                    LunaLog.Debug("Skipping backups: No players online.");
                }

                try
                {
                    await Task.Delay(IntervalSettings.SettingsStore.BackupIntervalMs, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}

using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using Server.System.VesselRelay;
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
                LunaLog.Debug("Performing backups...");
                VesselStoreSystem.BackupVessels();
                WarpSystem.SaveLatestSubspaceToFile();
                ScenarioStoreSystem.BackupScenarios();
                VesselRelaySystemDataBase.ShrinkDatabase();
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

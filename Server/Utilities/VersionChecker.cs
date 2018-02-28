using LunaCommon;
using LunaUpdater;
using Server.Context;
using Server.Log;
using System;
using System.Threading.Tasks;

namespace Server.Utilities
{
    public class VersionChecker
    {
        private static Version LatestVersion { get; set; }

        public static async void RefreshLatestVersion()
        {
            while (ServerContext.ServerRunning)
            {
                LatestVersion = UpdateChecker.GetLatestVersion();

                //Sleep for 30 minutes...
                await Task.Delay(30 * 60 * 1000);
            }
        }

        public static async void DisplayNewVersionMsg()
        {
            while (ServerContext.ServerRunning)
            {
                if (LatestVersion > new Version(LmpVersioning.CurrentVersion))
                {
                    LunaLog.Warning($"There is a new version of LMP! Please download it! Current: {LmpVersioning.CurrentVersion} Latest: {LatestVersion}");
                    LunaLog.Warning("You won't be listed in the master servers if you don't have the latest version!");
                }

                //Sleep for 30 seconds...
                await Task.Delay(30 * 1000);
            }
        }
    }
}

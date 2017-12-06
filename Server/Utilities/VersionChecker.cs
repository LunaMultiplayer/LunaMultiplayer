using LunaCommon;
using LunaUpdater;
using Server.Context;
using Server.Log;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utilities
{
    public class VersionChecker
    {
        private static Version LatestVersion { get; set; }

        public static void CheckForNewVersions()
        {
            Task.Run(() => RefreshLatestVersion());
            Task.Run(() => DisplayNewVersionMsg());
        }

        private static void RefreshLatestVersion()
        {
            while (ServerContext.ServerRunning)
            {
                LatestVersion = UpdateChecker.GetLatestVersion();

                //Sleep for 30 minutes...
                Thread.Sleep(30 * 60 * 1000);
            }
        }

        private static void DisplayNewVersionMsg()
        {
            while (ServerContext.ServerRunning)
            {
                if (LatestVersion > new Version(LmpVersioning.CurrentVersion))
                {
                    LunaLog.Warning($"There is a new version of LMP! Please download it! Current: {LmpVersioning.CurrentVersion} Latest: {LatestVersion}");
                }

                //Sleep for 30 seconds...
                Thread.Sleep(30 * 1000);
            }
        }
    }
}

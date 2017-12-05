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
                Task.Delay(30 * 60 * 1000);
            }
        }

        private static void DisplayNewVersionMsg()
        {
            while (ServerContext.ServerRunning)
            {
                if (LatestVersion > new Version(LmpVersioning.CurrentVersion))
                {
                    LunaLog.Warning("Found a new updated version! Please download it!");
                }

                //Sleep for 30 seconds...
                Task.Delay(30 * 1000);
            }
        }
    }
}

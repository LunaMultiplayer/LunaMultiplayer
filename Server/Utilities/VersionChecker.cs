using LunaCommon;
using LunaUpdater;
using Server.Context;
using System;
using System.Threading.Tasks;

namespace Server.Utilities
{
    public class VersionChecker
    {
        public static void CheckForNewVersions()
        {
            Task.Run(() =>
            {
                while (ServerContext.ServerRunning)
                {
                    var latestVersion = UpdateChecker.GetLatestVersion();
                    if (latestVersion > new Version(LmpVersioning.CurrentVersion))
                    {
                        ConsoleLogger.Log(LogLevels.Normal, "Found a new updated version! Please download it!");
                    }

                    //Sleep for 0.5 minute...
                    Task.Delay(30 * 1000);
                }
            });
        }
    }
}

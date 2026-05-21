using LmpCommon;
using LmpUpdater.Github;
using Server.Context;
using Server.Log;
using System;
using System.Threading.Tasks;

namespace Server.Utilities
{
    public class VersionChecker
    {
        private static Version LatestVersion { get; set; }

        public static async Task RefreshLatestVersionAsync()
        {
            while (ServerContext.ServerRunning)
            {
                LatestVersion = GithubUpdateChecker.GetLatestVersion();

                //Sleep for 30 minutes...
                await Task.Delay(30 * 60 * 1000);
            }
        }

        public static async Task DisplayNewVersionMsgAsync()
        {
            while (ServerContext.ServerRunning)
            {
                var multiplier = 1;
                if (LatestVersion > LmpVersioning.CurrentVersion)
                {
                    LunaLog.Info($"There is an update available for LMP, please download it when you're able to: {LmpVersioning.CurrentVersion} -> {LatestVersion}");
                    if (LmpVersioning.IsCompatible(LatestVersion))
                    {
                        LunaLog.Info($"This update is not required to stay compattibile with updated master servers and clients.");
                        multiplier = 60;
                    }
                    else
                    {
                        LunaLog.Warning("This update is mandatory to be shown on the server list once it's updated. You should go update");
                    }
                }

                // Repeat again in an hour if it's non-essential, or in a minute if it is essential.
                await Task.Delay(60 * 1000 * multiplier);
            }
        }
    }
}

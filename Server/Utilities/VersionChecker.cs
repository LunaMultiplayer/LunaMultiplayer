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
                // Repeat again in an hour if it's non-essential, or in a minute if it is essential.
                var delayMs = LmpVersioning.IsCompatible(LatestVersion)
                ? TimeSpan.FromHours(1)
                : TimeSpan.FromMinutes(1);

                if (LatestVersion > LmpVersioning.CurrentVersion)
                {
                    LunaLog.Info($"There is an update available for LMP, please download it when you're able to: {LmpVersioning.CurrentVersion} -> {LatestVersion}");
                    if (LmpVersioning.IsCompatible(LatestVersion))
                    {
                        LunaLog.Info($"This update is not required to stay compatible with updated master servers and clients.");
                    }
                    else
                    {
                        LunaLog.Warning("This update is required in order to be shown on the server list and to connect with clients running the new version.\n"
                        + "You should update the server ASAP.");
                    }
                }

                await Task.Delay(delayMs);
            }
        }
    }
}

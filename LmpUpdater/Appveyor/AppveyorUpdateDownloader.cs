using System;
using System.Linq;

namespace LmpUpdater.Appveyor
{
    public class AppveyorUpdateDownloader
    {
        public static string GetZipFileUrl(AppveyorProduct product, bool debugVersion = false)
        {
            if (AppveyorUpdateChecker.LatestBuild.build.status == "success")
            {
                var job = AppveyorUpdateChecker.LatestBuild.build.jobs.FirstOrDefault(j => debugVersion ? j.name.Contains("Debug") : j.name.Contains("Release"));
                if (job != null)
                {
                    var filename = GetProductFileName(product);
                    if (debugVersion)
                        filename += "-Debug.zip";
                    else
                        filename += "-Release.zip";

                    return $"https://ci.appveyor.com/api/buildjobs/{job.jobId}/artifacts/{filename}";
                }
            }

            return null;
        }

        private static string GetProductFileName(AppveyorProduct product)
        {
            switch (product)
            {
                case AppveyorProduct.Client:
                case AppveyorProduct.Server:
                    return "LunaMultiplayer";
                case AppveyorProduct.MasterServer:
                    return "LunaMultiplayerMasterServer";
                default:
                    throw new ArgumentOutOfRangeException(nameof(product), product, null);
            }
        }
    }
}

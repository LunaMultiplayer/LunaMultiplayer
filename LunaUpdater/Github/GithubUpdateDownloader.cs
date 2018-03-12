using System.Linq;

namespace LunaUpdater.Github
{
    public class GithubUpdateDownloader
    {
        public static string GetZipFileUrl(bool debugVersion = false)
        {
            if (debugVersion)
            {
                var debugAsset = GithubUpdateChecker.LatestRelease?.Assets.FirstOrDefault(a => a.Name.ToLower().Contains("debug"));
                return debugAsset?.BrowserDownloadUrl;
            }

            var releaseAsset = GithubUpdateChecker.LatestRelease?.Assets.FirstOrDefault(a => a.Name.ToLower().Contains("release"));
            return releaseAsset?.BrowserDownloadUrl;
        }
    }
}

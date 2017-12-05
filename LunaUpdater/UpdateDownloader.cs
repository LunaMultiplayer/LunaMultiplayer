using System.IO;
using System.Linq;
using System.Net;

namespace LunaUpdater
{
    public class UpdateDownloader
    {
        public static string GetZipFileUrl(bool debugVersion = false)
        {
            if (debugVersion)
            {
                var debugAsset = UpdateChecker.LatestRelease?.Assets.FirstOrDefault(a => a.Name.ToLower().Contains("debug"));
                return debugAsset?.BrowserDownloadUrl;
            }

            var releaseAsset = UpdateChecker.LatestRelease?.Assets.FirstOrDefault(a => a.Name.ToLower().Contains("release"));
            return releaseAsset?.BrowserDownloadUrl;
        }

        public static void DownloadZipFile(string url, string path)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return;

            using (var client = new WebClient())
            {
                client.DownloadFile(url, path);
            }
        }
    }
}

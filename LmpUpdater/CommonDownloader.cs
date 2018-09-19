using System;
using System.IO;
using System.Net;

namespace LmpUpdater
{
    public class CommonDownloader
    {
        public static bool DownloadZipFile(string url, string path)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(path) || !Directory.Exists(Path.GetDirectoryName(path)))
                return false;

            try
            {
                using (var client = new WebClient())
                {
                    client.DownloadFile(url, path);
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}

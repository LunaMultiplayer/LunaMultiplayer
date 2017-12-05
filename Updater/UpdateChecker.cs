using LmpGlobals;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using Updater.Contracts;

namespace Updater
{
    public class UpdateChecker
    {
        private static GitHubRelease _latestRelease;

        public static GitHubRelease LatestRelease
        {
            get
            {
                if (_latestRelease == null)
                {
                    try
                    {
                        using (var wc = new WebClient())
                        {
                            wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                            var json = wc.DownloadString(RepoConstants.LatestReleaseUrl);
                            if (JsonConvert.DeserializeObject(json, typeof(GitHubRelease)) is GitHubRelease gitHubRelease)
                                _latestRelease = gitHubRelease;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                return _latestRelease;
            }
        }

        public static Version GetLatestVersion()
        {
            return LatestRelease != null ? 
                new Version(new string(LatestRelease.TagName.Where(c => char.IsDigit(c) || char.IsPunctuation(c)).ToArray())) : 
                new Version("0.0.0");
        }
    }
}

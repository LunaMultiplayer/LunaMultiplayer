using LmpGlobal;
using LunaUpdater.Contracts;
using System;
using System.Linq;
using System.Net;
using System.Web.Script.Serialization;

namespace LunaUpdater
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
                            _latestRelease = new JavaScriptSerializer().Deserialize<GitHubRelease>(json);
                        }
                    }
                    catch (Exception)
                    {
                        //Ignore as either we don't have internet connection or something like that...
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

using LmpGlobal;
using LmpUpdater.Github.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;

namespace LmpUpdater.Github
{
    public class GithubUpdateChecker
    {
        public static GitHubRelease LatestRelease
        {
            get
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                        var json = wc.DownloadString(RepoConstants.ApiLatestGithubReleaseUrl);

                        return JsonConvert.DeserializeObject<GitHubRelease>(JObject.Parse(json).ToString());
                    }
                }
                catch (Exception)
                {
                    //Ignore as either we don't have internet connection or something like that...
                }

                return null;
            }
        }

        public static Version GetLatestVersion()
        {
            var jsonObj = LatestRelease;
            if (jsonObj == null) return new Version("0.0.0");

            var version = new string(jsonObj.TagName.Where(c => char.IsDigit(c) || char.IsPunctuation(c)).ToArray()).Split('.');

            return version.Length == 3 ?
                new Version(int.Parse(version[0]), int.Parse(version[1]), int.Parse(version[2])) :
                new Version("0.0.0");
        }
    }
}

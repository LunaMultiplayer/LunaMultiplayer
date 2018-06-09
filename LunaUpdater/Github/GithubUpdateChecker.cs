using JsonFx.Json;
using JsonFx.Serialization;
using JsonFx.Serialization.Resolvers;
using LmpGlobal;
using LunaUpdater.Github.Contracts;
using System;
using System.Linq;
using System.Net;

namespace LunaUpdater.Github
{
    public class GithubUpdateChecker
    {
        private static readonly JsonReader Reader = new JsonReader(new DataReaderSettings(new DataContractResolverStrategy()));

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
                        return Reader.Read<GitHubRelease>(json);
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
            return LatestRelease != null ?
                new Version(new string(LatestRelease.TagName.Where(c => char.IsDigit(c) || char.IsPunctuation(c)).ToArray())) :
                new Version("0.0.0");
        }
    }
}

using LmpGlobal;
using LmpUpdater.Appveyor.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;

namespace LmpUpdater.Appveyor
{
    public class AppveyorUpdateChecker
    {
        public static RootObject LatestBuild
        {
            get
            {
                try
                {
                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

                        var json = wc.DownloadString(RepoConstants.AppveyorUrl);
                        return JsonConvert.DeserializeObject<RootObject>(JObject.Parse(json).ToString());
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
            var versionComponents = LatestBuild?.build.version.Split('.');

            return versionComponents != null && versionComponents.Length >= 3 ?
                new Version(int.Parse(versionComponents[0]), int.Parse(versionComponents[1]), int.Parse(versionComponents[2])) :
                new Version("0.0.0");
        }
    }
}

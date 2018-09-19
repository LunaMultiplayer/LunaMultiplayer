using LmpGlobal;
using LmpClient.Windows.Update;
using LmpCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient.Utilities
{
    public static class UpdateHandler
    {
        private static readonly WaitForSeconds Wait = new WaitForSeconds(0.1f);

        public static IEnumerator CheckForUpdates()
        {
            using (var www = new WWW(RepoConstants.ApiLatestGithubReleaseUrl))
            {
                while (!www.isDone)
                {
                    yield return Wait;
                }
                if (www.error == null)
                {
                    if (!(Json.Deserialize(www.text) is Dictionary<string, object> data))
                        yield break;

                    var latestVersion = new Version(data["tag_name"].ToString());
                    LunaLog.Log($"Latest version: {latestVersion}");
                    if (latestVersion > LmpVersioning.CurrentVersion)
                    {
                        using (var www2 = new WWW(data["url"].ToString()))
                        {
                            while (!www2.isDone)
                            {
                                yield return Wait;
                            }
                            if (www2.error == null)
                            {
                                var changelog = data["body"].ToString();
                                UpdateWindow.LatestVersion = latestVersion;
                                UpdateWindow.Changelog = changelog;

                                UpdateWindow.Singleton.Display = true;
                            }
                        }
                    }
                }
                else
                {
                    LunaLog.Log($"Could not check for latest version. Error: {www.error}");
                }
            }
        }
    }
}

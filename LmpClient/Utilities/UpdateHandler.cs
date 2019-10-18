using LmpClient.Windows.Update;
using LmpCommon;
using LmpGlobal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace LmpClient.Utilities
{
    public static class UpdateHandler
    {
        public static IEnumerator CheckForUpdates()
        {
            using (var www = UnityWebRequest.Get(RepoConstants.ApiLatestGithubReleaseUrl))
            {
                yield return www.SendWebRequest();
                if (!www.isNetworkError || !www.isHttpError)
                {
                    if (!(Json.Deserialize(www.downloadHandler.text) is Dictionary<string, object> data))
                        yield break;

                    var latestVersion = new Version(data["tag_name"].ToString());
                    LunaLog.Log($"Latest version: {latestVersion}");
                    if (latestVersion > LmpVersioning.CurrentVersion)
                    {
                        using (var www2 = new UnityWebRequest(data["url"].ToString()))
                        {
                            yield return www2.SendWebRequest();
                            if (!www2.isNetworkError)
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

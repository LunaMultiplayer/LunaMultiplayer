using LmpGlobal;
using LunaClient.Utilities;
using LunaClient.Windows.Update;
using LunaCommon;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Update
{
    public class UpdateHandler
    {
        private static readonly WaitForSeconds Wait = new WaitForSeconds(0.1f);
        private static Version CurrentVersion => new Version(LmpVersioning.CurrentVersion);

        public static IEnumerator CheckForUpdates()
        {
            using (var www = new WWW(RepoConstants.LatestGithubReleaseUrl))
            {
                while (!www.isDone)
                {
                    yield return Wait;
                }
                if (www.error == null)
                {
                    var data = Json.Deserialize(www.text) as Dictionary<string, object>;
                    var latestVersion = new Version(data["tag_name"].ToString());
                    LunaLog.Log($"Latest version: {latestVersion}");
                    if (latestVersion > CurrentVersion)
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
                                UpdateWindow.CurrentVersion = CurrentVersion.ToString();
                                UpdateWindow.LatestVersion = latestVersion.ToString();
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

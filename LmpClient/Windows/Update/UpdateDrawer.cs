using LmpClient.Localization;
using LmpCommon;
using LmpGlobal;
using UnityEngine;

namespace LmpClient.Windows.Update
{
    public partial class UpdateWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.Text}", LmpVersioning.IsCompatible(LatestVersion) ? BoldGreenLabelStyle : BoldRedLabelStyle);

            GUILayout.BeginVertical();
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.CurrentVersion} {LmpVersioning.CurrentVersion}");
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.LatestVersion} {LatestVersion}");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (LmpVersioning.IsCompatible(LatestVersion))
                GUILayout.Label($"{LocalizationContainer.UpdateWindowText.StillCompatible}", BoldGreenLabelStyle);
            else
                GUILayout.Label($"{LocalizationContainer.UpdateWindowText.NotCompatible}", BoldRedLabelStyle);
            GUILayout.EndVertical();

            GUILayout.Label(LocalizationContainer.UpdateWindowText.Changelog);

            GUILayout.BeginVertical();
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5), GUILayout.Height(WindowHeight - 100));
            GUILayout.Label(Changelog);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button(DownloadBigIcon))
            {
                Application.OpenURL(RepoConstants.LatestGithubReleaseUrl);
                Display = false;
            }

            GUILayout.EndVertical();
        }
    }
}

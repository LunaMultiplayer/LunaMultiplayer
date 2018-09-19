using LmpGlobal;
using LmpClient.Localization;
using LmpCommon;
using UnityEngine;

namespace LmpClient.Windows.Update
{
    public partial class UpdateWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.Text}", LmpVersioning.IsCompatible(LatestVersion) ? BoldGreenLabelStyle : BoldRedLabelStyle);

            GUILayout.BeginVertical(BoxStyle);
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.CurrentVersion} {LmpVersioning.CurrentVersion}");
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.LatestVersion} {LatestVersion}");
            GUILayout.EndVertical();

            GUILayout.BeginVertical(BoxStyle);
            if (LmpVersioning.IsCompatible(LatestVersion))
                GUILayout.Label($"{LocalizationContainer.UpdateWindowText.StillCompatible}", BoldGreenLabelStyle);
            else
                GUILayout.Label($"{LocalizationContainer.UpdateWindowText.NotCompatible}", BoldRedLabelStyle);
            GUILayout.EndVertical();

            GUILayout.Label(LocalizationContainer.UpdateWindowText.Changelog);

            GUILayout.BeginVertical(BoxStyle);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5), GUILayout.Height(WindowHeight - 100));
            GUILayout.Label(Changelog);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (GUILayout.Button(DownloadBigIcon, ButtonStyle))
            {
                Application.OpenURL(RepoConstants.LatestGithubReleaseUrl);
                Display = false;
            }

            GUILayout.EndVertical();
        }
    }
}

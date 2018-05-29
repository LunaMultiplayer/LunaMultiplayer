using LmpGlobal;
using LunaClient.Localization;
using UnityEngine;

namespace LunaClient.Windows.Update
{
    public partial class UpdateWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.Label(LocalizationContainer.UpdateWindowText.Text, BoldLabelStyle);

            GUILayout.BeginVertical(BoxStyle);
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.CurrentVersion} {CurrentVersion}");
            GUILayout.Label($"{LocalizationContainer.UpdateWindowText.LatestVersion} {LatestVersion}");
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

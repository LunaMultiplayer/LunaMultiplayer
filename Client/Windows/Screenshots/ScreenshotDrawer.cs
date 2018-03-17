using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Screenshots
{
    public partial class ScreenshotsWindow
    {
        #region Folders

        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            //Draw the player buttons
            FoldersScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            foreach (var folderName in System.MiniatureImages.Keys.ToArray())
                DrawFolderButton(folderName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawFolderButton(string folderName)
        {
            if (GUILayout.Toggle(SelectedFolder == folderName, folderName, ButtonStyle))
            {
                if (SelectedFolder != folderName)
                {
                    SelectedFolder = folderName;
                    System.MessageSender.RequestMiniatures(SelectedFolder);
                }
            }
            else
            {
                if (SelectedFolder == folderName) SelectedFolder = null;
            }
        }

        #endregion

        public void DrawLibraryContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            LibraryScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            if (System.MiniatureImages.TryGetValue(SelectedFolder, out var miniatures))
            {
                var miniaturesList = miniatures.Values.ToArray();
                for (var i = 0; i < miniaturesList.Length; i += 3)
                {
                    GUILayout.BeginHorizontal();
                    DrawMiniature(miniaturesList[i]);
                    if (miniaturesList.Length > i + 1) DrawMiniature(miniaturesList[i + 1]);
                    if (miniaturesList.Length > i + 2) DrawMiniature(miniaturesList[i + 2]);
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawMiniature(LunaClient.Systems.Screenshot.Screenshot miniature)
        {
            GUILayout.FlexibleSpace();
            GUILayout.Button(miniature.Texture, ButtonStyle, GUILayout.Width(miniature.Width), GUILayout.Height(miniature.Height));
            GUILayout.FlexibleSpace();
        }
    }
}
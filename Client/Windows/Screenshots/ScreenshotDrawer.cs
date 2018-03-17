using LunaClient.Systems.Screenshot;
using System;
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

            DrawRefreshButton(() => System.MessageSender.RequestFolders(), () => Display = false);
            GUILayout.Space(15);

            FoldersScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            foreach (var folderName in System.MiniatureImages.Keys.ToArray())
                DrawFolderButton(folderName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawRefreshButton(Action refreshAction, Action closeAction)
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon, ButtonStyle)) refreshAction.Invoke();
            if (GUILayout.Button(CloseIcon, ButtonStyle)) closeAction.Invoke();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
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

        #region Image library

        public void DrawLibraryContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            DrawRefreshButton(() => System.MessageSender.RequestMiniatures(SelectedFolder), ()=> SelectedFolder = null);
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(SelectedFolder)) return;

            LibraryScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            if (System.MiniatureImages.TryGetValue(SelectedFolder, out var miniatures))
            {
                var miniaturesList = miniatures.Values.ToArray();
                for (var i = 0; i < miniaturesList.Length; i += 4)
                {
                    GUILayout.BeginHorizontal();
                    DrawMiniature(miniaturesList[i]);
                    if (miniaturesList.Length > i + 1) DrawMiniature(miniaturesList[i + 1]);
                    if (miniaturesList.Length > i + 2) DrawMiniature(miniaturesList[i + 2]);
                    if (miniaturesList.Length > i + 3) DrawMiniature(miniaturesList[i + 3]);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawMiniature(Screenshot miniature)
        {
            if (GUILayout.Button(miniature.Texture, ButtonStyle, GUILayout.Width(miniature.Width), GUILayout.Height(miniature.Height)))
            {
                SelectedImage = miniature.DateTaken;
                if(System.DownloadedImages.TryGetValue(SelectedFolder, out var downloadedImages) && !downloadedImages.ContainsKey(SelectedImage))
                    System.MessageSender.RequestImage(SelectedFolder, SelectedImage);
            }
        }

        #endregion

        #region Image viewer

        public void DrawImageContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SaveIcon, ButtonStyle))
            {
                System.SaveImage(SelectedFolder, SelectedImage);
                //Close after saving
                SelectedImage = 0;
            }
            if (GUILayout.Button(CloseIcon, ButtonStyle))
            {
                SelectedImage = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            if (SelectedImage == 0) return;

            ImageScrollPos = GUILayout.BeginScrollView(ImageScrollPos, ScrollStyle);
            if (System.DownloadedImages.TryGetValue(SelectedFolder, out var imagesDictionary) && imagesDictionary.TryGetValue(SelectedImage, out var screenShot))
            {
                GUILayout.Label(screenShot.Texture);
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        #endregion
    }
}
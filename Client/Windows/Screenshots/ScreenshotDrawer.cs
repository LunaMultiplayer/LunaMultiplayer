using LunaClient.Systems.Screenshot;
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
            DrawRefreshButton(() => System.MessageSender.RequestFolders());
            GUILayout.Space(15);

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

        #region Image library

        public void DrawLibraryContent(int windowId)
        {            
            //Always draw close button first
            DrawCloseButton(()=> SelectedFolder = null, LibraryWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.MessageSender.RequestMiniatures(SelectedFolder));
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(SelectedFolder)) return;

            LibraryScrollPos = GUILayout.BeginScrollView(LibraryScrollPos, ScrollStyle);
            if (System.MiniatureImages.TryGetValue(SelectedFolder, out var miniatures))
            {
                var miniaturesList = miniatures.Values.OrderBy(m => m.DateTaken).ToArray();
                if (!miniaturesList.Any())
                {
                    DrawWaitIcon();
                }
                for (var i = 0; i < miniaturesList.Length; i += 4)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    DrawMiniature(miniaturesList[i]);
                    GUILayout.FlexibleSpace();

                    if (miniaturesList.Length > i + 1)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(miniaturesList[i + 1]);
                        GUILayout.FlexibleSpace();
                    }

                    if (miniaturesList.Length > i + 2)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(miniaturesList[i + 2]);
                        GUILayout.FlexibleSpace();
                    }

                    if (miniaturesList.Length > i + 3)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(miniaturesList[i + 3]);
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                DrawWaitIcon();
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
            //Always draw close button first
            DrawCloseButton(()=> SelectedImage = 0, ImageWindowRect);

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
            else
            {
                DrawWaitIcon();
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
        
        #endregion
    }
}

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

            GUILayout.BeginVertical(BoxStyle);
            FoldersScrollPos = GUILayout.BeginScrollView(FoldersScrollPos, ScrollStyle);
            foreach (var folderName in System.MiniatureImages.Keys)
                DrawFolderButton(folderName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

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

            GUILayout.BeginVertical(BoxStyle);
            LibraryScrollPos = GUILayout.BeginScrollView(LibraryScrollPos, ScrollStyle);
            if (Miniatures.Any())
            {
                for (var i = 0; i < Miniatures.Count; i += 4)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.FlexibleSpace();
                    DrawMiniature(Miniatures[i]);
                    GUILayout.FlexibleSpace();

                    if (Miniatures.Count > i + 1)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(Miniatures[i + 1]);
                        GUILayout.FlexibleSpace();
                    }

                    if (Miniatures.Count > i + 2)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(Miniatures[i + 2]);
                        GUILayout.FlexibleSpace();
                    }

                    if (Miniatures.Count > i + 3)
                    {
                        GUILayout.FlexibleSpace();
                        DrawMiniature(Miniatures[i + 3]);
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                DrawWaitIcon(false);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
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

            GUILayout.BeginVertical(BoxStyle);
            ImageScrollPos = GUILayout.BeginScrollView(ImageScrollPos, ScrollStyle);
            if (System.DownloadedImages.TryGetValue(SelectedFolder, out var imagesDictionary) && imagesDictionary.TryGetValue(SelectedImage, out var screenShot))
            {
                GUILayout.Label(screenShot.Texture);
            }
            else
            {
                DrawWaitIcon(false);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }
        
        #endregion
    }
}

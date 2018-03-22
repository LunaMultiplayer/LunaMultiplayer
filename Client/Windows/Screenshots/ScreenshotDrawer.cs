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
            _foldersScrollPos = GUILayout.BeginScrollView(_foldersScrollPos, ScrollStyle);
            foreach (var folderName in System.MiniatureImages.Keys)
                DrawFolderButton(folderName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void DrawFolderButton(string folderName)
        {
            if (GUILayout.Toggle(_selectedFolder == folderName, folderName, ButtonStyle))
            {
                if (_selectedFolder != folderName)
                {
                    _selectedFolder = folderName;
                    System.MessageSender.RequestMiniatures(_selectedFolder);
                }
            }
            else
            {
                if (_selectedFolder == folderName) _selectedFolder = null;
            }
        }

        #endregion

        #region Image library

        public void DrawLibraryContent(int windowId)
        {            
            //Always draw close button first
            DrawCloseButton(()=> _selectedFolder = null, _libraryWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.MessageSender.RequestMiniatures(_selectedFolder));
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(_selectedFolder)) return;

            GUILayout.BeginVertical(BoxStyle);
            _libraryScrollPos = GUILayout.BeginScrollView(_libraryScrollPos, ScrollStyle);
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
                _selectedImage = miniature.DateTaken;
                if(System.DownloadedImages.TryGetValue(_selectedFolder, out var downloadedImages) && !downloadedImages.ContainsKey(_selectedImage))
                    System.MessageSender.RequestImage(_selectedFolder, _selectedImage);
            }
        }
        
        #endregion

        #region Image viewer

        public void DrawImageContent(int windowId)
        {            
            //Always draw close button first
            DrawCloseButton(()=> _selectedImage = 0, _imageWindowRect);
            if (GUI.RepeatButton(new Rect(_imageWindowRect.width - 15, _imageWindowRect.height - 15, 10, 10), ResizeIcon, ResizeButtonStyle))
            {
                ResizingWindow = true;
            }

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SaveIcon, ButtonStyle))
            {
                System.SaveImage(_selectedFolder, _selectedImage);
                //Close after saving
                _selectedImage = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            if (_selectedImage == 0) return;

            GUILayout.BeginVertical(BoxStyle);
            _imageScrollPos = GUILayout.BeginScrollView(_imageScrollPos, ScrollStyle);
            if (System.DownloadedImages.TryGetValue(_selectedFolder, out var imagesDictionary) && imagesDictionary.TryGetValue(_selectedImage, out var screenShot))
            {
                DrawImageCentered(screenShot);
            }
            else
            {
                DrawWaitIcon(false);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private static void DrawImageCentered(Screenshot screenShot)
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(screenShot.Texture);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        #endregion
    }
}

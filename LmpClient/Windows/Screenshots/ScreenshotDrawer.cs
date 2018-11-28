using LmpClient.Systems.Screenshot;
using System.Text;
using UniLinq;
using UnityEngine;

namespace LmpClient.Windows.Screenshots
{
    public partial class ScreenshotsWindow
    {
        #region Folders

        private static string _screenshotKeyLabel;
        private static string ScreenshotKeyLabel
        {
            get
            {
                if (string.IsNullOrEmpty(_screenshotKeyLabel))
                {
                    var sb = new StringBuilder();
                    sb.Append("Screenshot key: ");
                    if (!GameSettings.TAKE_SCREENSHOT.primary.isNone)
                    {
                        sb.Append(GameSettings.TAKE_SCREENSHOT.primary.code);
                    }

                    if (!GameSettings.TAKE_SCREENSHOT.secondary.isNone)
                    {
                        if (!GameSettings.TAKE_SCREENSHOT.primary.isNone)
                            sb.Append("/");
                        sb.Append(GameSettings.TAKE_SCREENSHOT.secondary.code);
                    }

                    if (GameSettings.TAKE_SCREENSHOT.primary.isNone && GameSettings.TAKE_SCREENSHOT.secondary.isNone)
                        sb.Append("NONE!");

                    _screenshotKeyLabel = sb.ToString();
                }

                return _screenshotKeyLabel;
            }
        }

        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() => System.MessageSender.RequestFolders());
            GUILayout.Label(ScreenshotKeyLabel);
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            _foldersScrollPos = GUILayout.BeginScrollView(_foldersScrollPos);
            foreach (var folderName in System.MiniatureImages.Keys)
                DrawFolderButton(folderName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
        }

        private void DrawFolderButton(string folderName)
        {
            if (GUILayout.Toggle(_selectedFolder == folderName, folderName, GetFolderStyle(folderName)))
            {
                if (_selectedFolder != folderName)
                {
                    _selectedFolder = folderName;

                    System.RequestMiniaturesIfNeeded(_selectedFolder);
                    _selectedImage = 0;
                    Miniatures.Clear();
                }
            }
            else
            {
                if (_selectedFolder == folderName)
                {
                    _selectedFolder = null;
                    Miniatures.Clear();
                }
            }
        }

        private GUIStyle GetFolderStyle(string folderName)
        {
            return System.FoldersWithNewContent.Contains(folderName) ? RedFontButtonStyle : Skin.button;
        }

        #endregion

        #region Image library

        public void DrawLibraryContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _selectedFolder = null, _libraryWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            DrawRefreshButton(() =>
            {
                _selectedImage = 0;
                System.MessageSender.RequestMiniatures(_selectedFolder);
                Miniatures.Clear();
            });
            GUILayout.Space(15);

            if (string.IsNullOrEmpty(_selectedFolder)) return;

            GUILayout.BeginVertical();
            _libraryScrollPos = GUILayout.BeginScrollView(_libraryScrollPos);
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

        private static void DrawMiniature(Screenshot miniature)
        {
            if (GUILayout.Button(miniature.Texture, GUILayout.Width(miniature.Width), GUILayout.Height(miniature.Height)))
            {
                _selectedImage = miniature.DateTaken;
            }
        }

        #endregion

        #region Image viewer

        public void DrawImageContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _selectedImage = 0, _imageWindowRect);
            if (GUI.RepeatButton(new Rect(_imageWindowRect.width - 15, _imageWindowRect.height - 15, 10, 10), ResizeIcon, ResizeButtonStyle))
            {
                ResizingWindow = true;
            }

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(SaveIcon))
            {
                System.SaveImage(_selectedFolder, _selectedImage);
                //Close after saving
                _selectedImage = 0;
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            if (_selectedImage == 0) return;

            GUILayout.BeginVertical();
            _imageScrollPos = GUILayout.BeginScrollView(_imageScrollPos);
            if (System.DownloadedImages.TryGetValue(_selectedFolder, out var imagesDictionary) && imagesDictionary.TryGetValue(_selectedImage, out var screenShot))
            {
                DrawImageCentered(screenShot);
            }
            else
            {
                DrawWaitIcon(false);
                System.MessageSender.RequestImage(_selectedFolder, _selectedImage);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            if (Miniatures.Count > 1)
            {   
                //Draw screenshot cycle buttons if we have more than 1 screenshot
                GUILayout.Space(15);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(CycleFirstIcon))
                {
                    //Load first image.
                    _selectedImage = Miniatures[0].DateTaken;
                }
                if (GUILayout.Button(CyclePreviousIcon))
                {
                    //Load previous image. If start of index, load last image.
                    _selectedImage = Miniatures[CurrentIndex > 0 ? CurrentIndex - 1 : Miniatures.Count - 1].DateTaken;
                }
                if (GUILayout.Button(CycleNextIcon))
                {
                    //Load next image. If end of index, load first image.
                    _selectedImage = Miniatures[CurrentIndex < Miniatures.Count - 1 ? CurrentIndex + 1 : 0].DateTaken;
                }
                if (GUILayout.Button(CycleLastIcon))
                {
                    //Load last image.
                    _selectedImage = Miniatures[Miniatures.Count - 1].DateTaken;
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.Space(15);
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

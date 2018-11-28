using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.Screenshot;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LmpClient.Windows.Screenshots
{
    public partial class ScreenshotsWindow : SystemWindow<ScreenshotsWindow, ScreenshotSystem>
    {
        #region Fields

        private const int UpdateIntervalMs = 1500;

        protected const float FoldersWindowHeight = 300;
        protected const float FoldersWindowWidth = 200;
        protected const float LibraryWindowHeight = 600;
        protected const float LibraryWindowWidth = 600;
        protected const float ImageWindowHeight = 762;
        protected const float ImageWindowWidth = 1024;

        private static Rect _libraryWindowRect;
        private static Rect _imageWindowRect;

        private static GUILayoutOption[] _foldersLayoutOptions;
        private static GUILayoutOption[] _libraryLayoutOptions;

        private static Vector2 _foldersScrollPos;
        private static Vector2 _libraryScrollPos;
        private static Vector2 _imageScrollPos;

        private static string _selectedFolder;
        private static long _selectedImage;

        private static DateTime _lastGuiUpdateTime = DateTime.MinValue;
        private static readonly List<Screenshot> Miniatures = new List<Screenshot>();

        private static int CurrentIndex => _selectedImage != 0 ? Miniatures.FindIndex(s => s.DateTaken.Equals(_selectedImage)) : 0;

        #endregion

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display&& MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set
            {
                if (!value) Reset();

                if (value && !_display && System.MiniatureImages.Count == 0)
                    System.MessageSender.RequestFolders();
                base.Display = _display = value;
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Display) return;

            if (TimeUtil.IsInInterval(ref _lastGuiUpdateTime, UpdateIntervalMs))
            {
                Miniatures.Clear();
                if (!string.IsNullOrEmpty(_selectedFolder) && System.MiniatureImages.TryGetValue(_selectedFolder, out var miniatures))
                {
                    Miniatures.AddRange(miniatures.Values.OrderBy(v => v.DateTaken));
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                ResizingWindow = false;
            }

            if (ResizingWindow)
            {
                _imageWindowRect.width = Input.mousePosition.x - _imageWindowRect.x + 10;
                _imageWindowRect.height = Screen.height - Input.mousePosition.y - _imageWindowRect.y + 10;
            }
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6719 + MainSystem.WindowOffset,
                WindowRect, DrawContent, LocalizationContainer.ScreenshotWindowText.Folders, _foldersLayoutOptions));

            if (!string.IsNullOrEmpty(_selectedFolder) && System.MiniatureImages.ContainsKey(_selectedFolder))
            {
                _libraryWindowRect = FixWindowPos(GUILayout.Window(6720 + MainSystem.WindowOffset, _libraryWindowRect,
                    DrawLibraryContent, $"{_selectedFolder} {LocalizationContainer.ScreenshotWindowText.Screenshots}", _libraryLayoutOptions));
            }

            if (!string.IsNullOrEmpty(_selectedFolder) && System.DownloadedImages.ContainsKey(_selectedFolder) && _selectedImage > 0)
            {
                _imageWindowRect = FixWindowPos(GUILayout.Window(6721 + MainSystem.WindowOffset, _imageWindowRect,
                    DrawImageContent, $"{_selectedFolder} [{CurrentIndex + 1}] - {DateTime.FromBinary(_selectedImage):yyyy/MM/dd HH:mm:ss} UTC"));
            }
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(50, Screen.height / 2f - FoldersWindowHeight / 2f, FoldersWindowWidth, FoldersWindowHeight);
            _libraryWindowRect = new Rect(Screen.width / 2f - LibraryWindowWidth / 2f, Screen.height / 2f - LibraryWindowHeight / 2f, LibraryWindowWidth, LibraryWindowHeight);
            _imageWindowRect = new Rect(Screen.width / 2f - ImageWindowWidth, Screen.height / 2f - ImageWindowHeight / 2f, ImageWindowWidth, ImageWindowHeight);

            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            _foldersLayoutOptions = new GUILayoutOption[4];
            _foldersLayoutOptions[0] = GUILayout.MinWidth(FoldersWindowWidth);
            _foldersLayoutOptions[1] = GUILayout.MaxWidth(FoldersWindowWidth);
            _foldersLayoutOptions[2] = GUILayout.MinHeight(FoldersWindowHeight);
            _foldersLayoutOptions[3] = GUILayout.MaxHeight(FoldersWindowHeight);

            _libraryLayoutOptions = new GUILayoutOption[4];
            _libraryLayoutOptions[0] = GUILayout.MinWidth(LibraryWindowWidth);
            _libraryLayoutOptions[1] = GUILayout.MaxWidth(LibraryWindowWidth);
            _libraryLayoutOptions[2] = GUILayout.MinHeight(LibraryWindowHeight);
            _libraryLayoutOptions[3] = GUILayout.MaxHeight(LibraryWindowHeight);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_ScreenshotLock");
            }
        }

        public override void CheckWindowLock()
        {
            if (Display)
            {
                if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
                {
                    RemoveWindowLock();
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = WindowRect.Contains(mousePos) 
                                 || !string.IsNullOrEmpty(_selectedFolder) && _libraryWindowRect.Contains(mousePos) 
                                 || _selectedImage > 0 && _imageWindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_ScreenshotLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }

        private static void Reset()
        {
            _selectedFolder = null;
            _selectedImage = 0;
        }
    }
}

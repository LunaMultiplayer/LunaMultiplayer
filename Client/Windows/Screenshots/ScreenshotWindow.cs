using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.Screenshot;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Screenshots
{
    public partial class ScreenshotsWindow : SystemWindow<ScreenshotsWindow, ScreenshotSystem>
    {
        #region Fields

        protected const float FoldersWindowHeight = 300;
        protected const float FoldersWindowWidth = 200;
        protected const float LibraryWindowHeight = 600;
        protected const float LibraryWindowWidth = 600;
        protected const float ImageWindowHeight = 762;
        protected const float ImageWindowWidth = 1024;

        protected Rect LibraryWindowRect { get; set; }
        protected Rect ImageWindowRect { get; set; }

        protected GUILayoutOption[] FoldersLayoutOptions { get; set; }
        protected GUILayoutOption[] LibraryLayoutOptions { get; set; }
        protected GUILayoutOption[] ImageLayoutOptions { get; set; }

        protected Vector2 FoldersScrollPos { get; set; }
        protected Vector2 LibraryScrollPos { get; set; }
        protected Vector2 ImageScrollPos { get; set; }

        private string SelectedFolder { get; set; }
        private long SelectedImage { get; set; } = 0;

        private static DateTime _lastGuiUpdateTime = DateTime.MinValue;
        private static readonly List<string> Folders = new List<string>();
        private static readonly List<Screenshot> Miniatures = new List<Screenshot>();
        #endregion

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set
            {
                if (value && !_display && System.MiniatureImages.Count == 0)
                    System.MessageSender.RequestFolders();
                base.Display = _display = value;
            }
        }

        public override void Update()
        {
            base.Update();
            SafeDisplay = Display;
            if (DateTime.Now - _lastGuiUpdateTime > TimeSpan.FromSeconds(2.5f))
            {
                _lastGuiUpdateTime = DateTime.Now;

                Folders.Clear();
                Folders.AddRange(System.MiniatureImages.Keys);

                Miniatures.Clear();
                if (!string.IsNullOrEmpty(SelectedFolder) && System.MiniatureImages.TryGetValue(SelectedFolder, out var miniatures))
                {
                    Miniatures.AddRange(miniatures.Values.OrderBy(v => v.DateTaken));
                }
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
            {
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6719 + MainSystem.WindowOffset,
                    WindowRect, DrawContent, LocalizationContainer.ScreenshotWindowText.Folders, WindowStyle, FoldersLayoutOptions));
            }

            if (SafeDisplay && !string.IsNullOrEmpty(SelectedFolder) && System.MiniatureImages.ContainsKey(SelectedFolder))
            {
                LibraryWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6720 + MainSystem.WindowOffset,
                    LibraryWindowRect, DrawLibraryContent, $"{SelectedFolder} {LocalizationContainer.ScreenshotWindowText.Screenshots}", WindowStyle,
                    LibraryLayoutOptions));
            }

            if (SafeDisplay && SelectedImage > 0 && System.DownloadedImages.ContainsKey(SelectedFolder))
            {
                ImageWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6721 + MainSystem.WindowOffset,
                    ImageWindowRect, DrawImageContent, $"{DateTime.FromBinary(SelectedImage).ToLongTimeString()}", WindowStyle,
                    ImageLayoutOptions));
            }

            CheckWindowLock();
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(50, Screen.height / 2f - FoldersWindowHeight / 2f, FoldersWindowWidth, FoldersWindowHeight);
            LibraryWindowRect = new Rect(Screen.width / 2f - LibraryWindowWidth / 2f, Screen.height / 2f - LibraryWindowHeight / 2f, LibraryWindowWidth, LibraryWindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            FoldersLayoutOptions = new GUILayoutOption[4];
            FoldersLayoutOptions[0] = GUILayout.MinWidth(FoldersWindowWidth);
            FoldersLayoutOptions[1] = GUILayout.MaxWidth(FoldersWindowWidth);
            FoldersLayoutOptions[2] = GUILayout.MinHeight(FoldersWindowHeight);
            FoldersLayoutOptions[3] = GUILayout.MaxHeight(FoldersWindowHeight);

            LibraryLayoutOptions = new GUILayoutOption[4];
            LibraryLayoutOptions[0] = GUILayout.MinWidth(LibraryWindowWidth);
            LibraryLayoutOptions[1] = GUILayout.MaxWidth(LibraryWindowWidth);
            LibraryLayoutOptions[2] = GUILayout.MinHeight(LibraryWindowHeight);
            LibraryLayoutOptions[3] = GUILayout.MaxHeight(LibraryWindowHeight);

            ImageLayoutOptions = new GUILayoutOption[4];
            ImageLayoutOptions[0] = GUILayout.MinWidth(ImageWindowWidth);
            ImageLayoutOptions[1] = GUILayout.MaxWidth(ImageWindowWidth);
            ImageLayoutOptions[2] = GUILayout.MinHeight(ImageWindowHeight);
            ImageLayoutOptions[3] = GUILayout.MaxHeight(ImageWindowHeight);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_ScreenshotLock");
            }
        }

        public void CheckWindowLock()
        {
            if (SafeDisplay)
            {
                if (MainSystem.NetworkState < ClientState.Running || HighLogic.LoadedSceneIsFlight)
                {
                    RemoveWindowLock();
                    return;
                }

                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = WindowRect.Contains(mousePos) || LibraryWindowRect.Contains(mousePos);
                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_ScreenshotLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!SafeDisplay && IsWindowLocked)
                RemoveWindowLock();
        }
    }
}

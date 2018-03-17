using LunaClient.Base;
using LunaClient.Systems.Screenshot;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
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

        protected Rect FoldersWindowRect { get; set; }
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

        private Texture2D RefreshIcon { get; set; }
        private Texture2D SaveIcon { get; set; }
        private Texture2D CloseIcon { get; set; }

        #endregion

        private static bool _display;
        public override bool Display
        {
            get => _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set
            {
                if (value && !_display)
                    System.MessageSender.RequestFolders();
                _display = value;
            }
        }

        public override void Update()
        {
            base.Update();
            SafeDisplay = Display;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
            {
                FoldersWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6719 + MainSystem.WindowOffset,
                    FoldersWindowRect, DrawContent, "LunaMultiplayer - Folder Library", WindowStyle, FoldersLayoutOptions));
            }

            if (SafeDisplay && !string.IsNullOrEmpty(SelectedFolder) && System.MiniatureImages.ContainsKey(SelectedFolder))
            {
                LibraryWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6720 + MainSystem.WindowOffset,
                    LibraryWindowRect, DrawLibraryContent, $"LunaMultiplayer - {SelectedFolder} Library", WindowStyle,
                    LibraryLayoutOptions));
            }

            if (SafeDisplay && SelectedImage > 0 && System.DownloadedImages.TryGetValue(SelectedFolder, out var imagesDictionary) && imagesDictionary.ContainsKey(SelectedImage))
            {
                ImageWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6721 + MainSystem.WindowOffset,
                    ImageWindowRect, DrawImageContent, $"{DateTime.FromBinary(SelectedImage).ToLongTimeString()}", WindowStyle,
                    ImageLayoutOptions));
            }

            CheckWindowLock();
        }

        public override void SetStyles()
        {
            CloseIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "close.png"), 16, 16);
            SaveIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "save.png"), 16, 16);
            RefreshIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Icons", "refresh.png"), 16, 16);

            FoldersWindowRect = new Rect(50, Screen.height / 2f - FoldersWindowHeight / 2f, FoldersWindowWidth, FoldersWindowHeight);
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

                var shouldLock = FoldersWindowRect.Contains(mousePos) || LibraryWindowRect.Contains(mousePos);
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
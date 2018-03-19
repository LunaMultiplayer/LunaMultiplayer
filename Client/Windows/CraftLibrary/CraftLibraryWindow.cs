using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow : SystemWindow<CraftLibraryWindow, CraftLibrarySystem>
    {
        #region Fields

        protected const float FoldersWindowHeight = 300;
        protected const float FoldersWindowWidth = 200;
        protected const float LibraryWindowHeight = 300;
        protected const float LibraryWindowWidth = 400;
        protected const float UploadWindowHeight = 300;
        protected const float UploadWindowWidth = 400;

        protected Rect LibraryWindowRect { get; set; }
        protected Rect UploadWindowRect { get; set; }

        protected GUILayoutOption[] FoldersLayoutOptions { get; set; }
        protected GUILayoutOption[] LibraryLayoutOptions { get; set; }
        protected GUILayoutOption[] UploadLayoutOptions { get; set; }

        protected Vector2 FoldersScrollPos { get; set; }
        protected Vector2 LibraryScrollPos { get; set; }
        protected Vector2 UploadScrollPos { get; set; }

        private string SelectedFolder { get; set; }
        private bool DrawUploadScreen { get; set; }

        private static DateTime _lastGuiUpdateTime = DateTime.MinValue;
        private static readonly List<string> Folders = new List<string>();
        private static readonly List<CraftBasicEntry> Crafts = new List<CraftBasicEntry>();
        private static readonly List<CraftEntry> OwnCrafts = new List<CraftEntry>();
        #endregion

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set
            {
                if (value && !_display && System.CraftInfo.Count == 0)
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
                Folders.AddRange(System.CraftInfo.Keys);

                Crafts.Clear();
                if (!string.IsNullOrEmpty(SelectedFolder) && System.CraftInfo.TryGetValue(SelectedFolder, out var craftsDictionary))
                {
                    Crafts.AddRange(craftsDictionary.Values);
                }
            }
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
            {
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6707 + MainSystem.WindowOffset,
                    WindowRect, DrawContent, LocalizationContainer.CraftLibraryWindowText.Folders, WindowStyle, FoldersLayoutOptions));
            }

            if (SafeDisplay && !string.IsNullOrEmpty(SelectedFolder) && System.CraftInfo.ContainsKey(SelectedFolder))
            {
                LibraryWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6708 + MainSystem.WindowOffset,
                    LibraryWindowRect, DrawLibraryContent, $"{SelectedFolder} {LocalizationContainer.CraftLibraryWindowText.Crafts}", WindowStyle,
                    LibraryLayoutOptions));
            }

            if (SafeDisplay && DrawUploadScreen)
            {
                UploadWindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6709 + MainSystem.WindowOffset,
                    UploadWindowRect, DrawUploadScreenContent, LocalizationContainer.CraftLibraryWindowText.Upload, WindowStyle,
                    UploadLayoutOptions));
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
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_CraftLibraryLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_CraftLibraryLock");
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

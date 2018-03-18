using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow : SystemWindow<CraftLibraryWindow, CraftLibrarySystem>
    {
        #region Fields

        protected const float WindowHeight = 300;
        protected const float WindowWidth = 200;
        protected const float LibraryWindowHeight = 400;
        protected const float LibraryWindowWidth = 300;

        protected bool ShowUpload { get; set; }
        
        protected Rect LibraryWindowRect { get; set; }
        
        protected GUILayoutOption[] LibraryLayoutOptions { get; set; }

        protected Vector2 PlayerScrollPos { get; set; }
        protected Vector2 LibraryScrollPos { get; set; }

        #endregion

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState >= ClientState.Running &&
                   HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => base.Display = _display = value;
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
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6707 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.CraftLibraryWindowText.Title, WindowStyle, LayoutOptions));
            if (SafeDisplay && System.SelectedPlayer != null)
                if (System.PlayersWithCrafts.Contains(System.SelectedPlayer) ||
                    System.SelectedPlayer == SettingsSystem.CurrentSettings.PlayerName)
                    LibraryWindowRect =
                        LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6708 + MainSystem.WindowOffset,
                            LibraryWindowRect, DrawLibraryContent,
                            $"LunaMultiplayer - {System.SelectedPlayer} Craft Library", WindowStyle,
                            LibraryLayoutOptions));
                else
                    System.SelectedPlayer = null;
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            //Setup GUI stuff
            //left 50, middle height
            WindowRect = new Rect(50, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            //middle of the screen
            LibraryWindowRect = new Rect(Screen.width / 2f - LibraryWindowWidth / 2f,
                Screen.height / 2f - LibraryWindowHeight / 2f, LibraryWindowWidth, LibraryWindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            LibraryLayoutOptions = new GUILayoutOption[4];
            LibraryLayoutOptions[0] = GUILayout.MinWidth(LibraryWindowWidth);
            LibraryLayoutOptions[1] = GUILayout.MaxWidth(LibraryWindowWidth);
            LibraryLayoutOptions[2] = GUILayout.MinHeight(LibraryWindowHeight);
            LibraryLayoutOptions[3] = GUILayout.MaxHeight(LibraryWindowHeight);
            
            TextAreaOptions = new GUILayoutOption[2];
            TextAreaOptions[0] = GUILayout.ExpandWidth(false);
            TextAreaOptions[1] = GUILayout.ExpandWidth(false);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_CraftLock");
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
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_CraftLock");
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
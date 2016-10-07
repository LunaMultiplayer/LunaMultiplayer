using LunaClient.Base;
using LunaClient.Systems.CraftLibrary;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow : SystemWindow<CraftLibraryWindow, CraftLibrarySystem>
    {
        public override CraftLibrarySystem System => CraftLibrarySystem.Singleton;

        public override void Update()
        {
            Display &= MainSystem.Singleton.GameRunning;
            SafeDisplay = Display;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                PlayerWindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6707 + MainSystem.WindowOffset, PlayerWindowRect,
                        DrawContent, "LunaMultiPlayer - Craft Library", WindowStyle, PlayerLayoutOptions));
            if (SafeDisplay && (System.SelectedPlayer != null))
                if (System.PlayersWithCrafts.Contains(System.SelectedPlayer) ||
                    (System.SelectedPlayer == SettingsSystem.CurrentSettings.PlayerName))
                    LibraryWindowRect =
                        LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6708 + MainSystem.WindowOffset,
                            LibraryWindowRect, DrawLibraryContent,
                            "LunaMultiPlayer - " + System.SelectedPlayer + " Craft Library", WindowStyle,
                            LibraryLayoutOptions));
                else
                    System.SelectedPlayer = null;
            CheckWindowLock();
        }

        public override void SetStyles()
        {
            //Setup GUI stuff
            //left 50, middle height
            PlayerWindowRect = new Rect(50, Screen.height/2f - PlayerWindowHeight/2f, PlayerWindowWidth,
                PlayerWindowHeight);
            //middle of the screen
            LibraryWindowRect = new Rect(Screen.width/2f - LibraryWindowWidth/2f,
                Screen.height/2f - LibraryWindowHeight/2f, LibraryWindowWidth, LibraryWindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            PlayerLayoutOptions = new GUILayoutOption[4];
            PlayerLayoutOptions[0] = GUILayout.MinWidth(PlayerWindowWidth);
            PlayerLayoutOptions[1] = GUILayout.MaxWidth(PlayerWindowWidth);
            PlayerLayoutOptions[2] = GUILayout.MinHeight(PlayerWindowHeight);
            PlayerLayoutOptions[3] = GUILayout.MaxHeight(PlayerWindowHeight);

            LibraryLayoutOptions = new GUILayoutOption[4];
            LibraryLayoutOptions[0] = GUILayout.MinWidth(LibraryWindowWidth);
            LibraryLayoutOptions[1] = GUILayout.MaxWidth(LibraryWindowWidth);
            LibraryLayoutOptions[2] = GUILayout.MinHeight(LibraryWindowHeight);
            LibraryLayoutOptions[3] = GUILayout.MaxHeight(LibraryWindowHeight);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            LabelStyle = new GUIStyle(GUI.skin.label);
            ScrollStyle = new GUIStyle(GUI.skin.scrollView);

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
            if (!MainSystem.Singleton.GameRunning)
            {
                RemoveWindowLock();
                return;
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                RemoveWindowLock();
                return;
            }

            if (SafeDisplay)
            {
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = PlayerWindowRect.Contains(mousePos) || LibraryWindowRect.Contains(mousePos);

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

        #region Fields

        protected const float PlayerWindowHeight = 300;
        protected const float PlayerWindowWidth = 200;
        protected const float LibraryWindowHeight = 400;
        protected const float LibraryWindowWidth = 300;

        protected bool ShowUpload { get; set; }

        protected Rect PlayerWindowRect { get; set; }
        protected Rect LibraryWindowRect { get; set; }

        protected GUILayoutOption[] PlayerLayoutOptions { get; set; }
        protected GUILayoutOption[] LibraryLayoutOptions { get; set; }

        protected Vector2 PlayerScrollPos { get; set; }
        protected Vector2 LibraryScrollPos { get; set; }

        #endregion
    }
}
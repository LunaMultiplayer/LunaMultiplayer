using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.Admin;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Admin
{
    public partial class AdminWindow : SystemWindow<AdminWindow,AdminSystem>
    {
        #region Fields
        
        private const float WindowHeight = 300;
        private const float WindowWidth = 400;
        private const float ConfirmationWindowHeight = 50;
        private const float ConfirmationWindowWidth = 350;

        private static Rect _confirmationWindowRect;
        private static GUILayoutOption[] _confirmationLayoutOptions;

        private static string _selectedPlayer;
        private static bool _banMode;
        private static string _reason = string.Empty;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER && SettingsSystem.ServerSettings.AllowAdmin;
            set => base.Display = _display = value;
        }

        protected override bool DisplayTooltips => true;

        #endregion

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6723 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.AdminWindowText.Title, WindowStyle, LayoutOptions));
                if (!string.IsNullOrEmpty(_selectedPlayer))
                {
                    _confirmationWindowRect = FixWindowPos(GUILayout.Window(6724 + MainSystem.WindowOffset,
                        _confirmationWindowRect, DrawConfirmationDialog, LocalizationContainer.AdminWindowText.ConfirmDialogTitle, WindowStyle, _confirmationLayoutOptions));
                }
            }
            else
            {
                _reason = string.Empty;
                _selectedPlayer = null;
            }

            CheckWindowLock();
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            _confirmationLayoutOptions = new GUILayoutOption[4];
            _confirmationLayoutOptions[0] = GUILayout.MinWidth(ConfirmationWindowWidth);
            _confirmationLayoutOptions[1] = GUILayout.MaxWidth(ConfirmationWindowWidth);
            _confirmationLayoutOptions[2] = GUILayout.MinHeight(ConfirmationWindowHeight);
            _confirmationLayoutOptions[3] = GUILayout.MaxHeight(ConfirmationWindowHeight);

            ScrollPos = new Vector2();
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_AdminLock");
            }
        }

        public void CheckWindowLock()
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
                                 || !string.IsNullOrEmpty(_selectedPlayer) && _confirmationWindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_AdminLock");
                    IsWindowLocked = true;
                }
                if (!shouldLock && IsWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && IsWindowLocked)
                RemoveWindowLock();
        }
    }
}

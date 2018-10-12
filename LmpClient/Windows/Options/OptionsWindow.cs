using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;
using UnityEngine;

namespace LmpClient.Windows.Options
{
    public partial class OptionsWindow : Window<OptionsWindow>
    {
        #region Fields
        
        private const float WindowHeight = 400;
        private const float WindowWidth = 300;
        private const float UniverseConverterWindowHeight = 300;
        private const float UniverseConverterWindowWidth = 200;

        private static Color _tempColor = new Color(1f, 1f, 1f, 1f);

        private static GUIStyle _tempColorLabelStyle;
        private static bool _showGeneralSettings;
        private static bool _showBadNetworkSimulationSettings;
        private static bool _showAdvancedNetworkSettings;
        private static bool _showClockOffsetSettings;
        private static bool _infiniteTimeout;

        private static Rect _universeConverterWindowRect;
        private static GUILayoutOption[] _universeConverterLayoutOptions;

        private static bool _displayUniverseConverterDialog;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display;
            set
            {
                if (!_display && value)
                {
                    _tempColor = SettingsSystem.CurrentSettings.PlayerColor;
                }

                base.Display = _display = value;
            }
        }

        #endregion

        private readonly GUILayoutOption[] _smallOption = { GUILayout.Width(20), GUILayout.ExpandWidth(false) };

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6711 + MainSystem.WindowOffset, WindowRect, DrawContent,
                LocalizationContainer.OptionsWindowText.Title, LayoutOptions));

            if (_displayUniverseConverterDialog)
            {
                _universeConverterWindowRect = FixWindowPos(GUILayout.Window(6712 + MainSystem.WindowOffset,
                    _universeConverterWindowRect, DrawUniverseConverterDialog, "Universe converter", _universeConverterLayoutOptions));
            }
        }
        
        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width / 2f - WindowWidth / 2f, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            _universeConverterWindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);

            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.Width(WindowWidth);
            LayoutOptions[1] = GUILayout.Height(WindowHeight);
            LayoutOptions[2] = GUILayout.ExpandWidth(true);
            LayoutOptions[3] = GUILayout.ExpandHeight(true);

            _universeConverterLayoutOptions = new GUILayoutOption[4];
            _universeConverterLayoutOptions[0] = GUILayout.Width(UniverseConverterWindowWidth);
            _universeConverterLayoutOptions[1] = GUILayout.Height(UniverseConverterWindowHeight);
            _universeConverterLayoutOptions[2] = GUILayout.ExpandWidth(true);
            _universeConverterLayoutOptions[3] = GUILayout.ExpandHeight(true);

            _tempColor = new Color();
            _tempColorLabelStyle = new GUIStyle(GUI.skin.label);
        }

        public override void RemoveWindowLock()
        {
            if (IsWindowLocked)
            {
                IsWindowLocked = false;
                InputLockManager.RemoveControlLock("LMP_OptionsLock");
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

                var shouldLock = WindowRect.Contains(mousePos);

                if (shouldLock && !IsWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "LMP_OptionsLock");
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

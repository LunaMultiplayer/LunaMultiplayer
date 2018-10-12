using LmpClient.Base;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpCommon;
using UnityEngine;

namespace LmpClient.Windows.Connection
{
    public partial class ConnectionWindow : Window<ConnectionWindow>
    {
        #region Fields

        #region Public

        private static int _selectedIndex;
        private static int SelectedIndex
        {
            get
            {
                if (SettingsSystem.CurrentSettings.Servers.Count == 0)
                    return -1;
                return _selectedIndex;
            }
            set => _selectedIndex = value;
        }

        #endregion

        protected const float WindowHeight = 400;
        protected const float WindowWidth = 400;

#if DEBUG
        private readonly string _title = $"Luna Multiplayer {LmpVersioning.CurrentVersion} Debug port: {CommonUtil.DebugPort}";
#else
        private readonly string _title = $"Luna Multiplayer {LmpVersioning.CurrentVersion}";
#endif

        #endregion

        #region Base overrides

        public override bool Display => SettingsSystem.CurrentSettings.DisclaimerAccepted && MainSystem.ToolbarShowGui && HighLogic.LoadedScene == GameScenes.MAINMENU;

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.9f - WindowWidth, Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            StatusStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            LabelOptions = new GUILayoutOption[1];
            LabelOptions[0] = GUILayout.Width(100);
        }

        protected override void OnCloseButton()
        {
            base.OnCloseButton();
            MainSystem.ToolbarShowGui = false;
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6702 + MainSystem.WindowOffset, WindowRect, DrawContent, _title));
        }

        #endregion
    }
}

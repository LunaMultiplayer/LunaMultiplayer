using LunaClient.Base;
using LunaClient.Systems;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Windows.Connection
{
    public partial class ConnectionWindow : Window<ConnectionWindow>
    {
        #region Fields

        #region Public

        public static bool Closed { get; set; } = false;
        public static bool ConnectEventHandled { get; set; } = true;
        public static bool DisconnectEventHandled { get; set; } = true;
        public static bool AddEventHandled { get; set; } = true;
        public static bool EditEventHandled { get; set; } = true;
        public static bool RemoveEventHandled { get; set; } = true;
        public static bool RenameEventHandled { get; set; } = true;
        public static bool AddingServer { get; set; }
        public static bool AddingServerSafe { get; set; }
        public static int Selected { get; set; } = -1;
        public static ServerEntry AddEntry { get; set; } = null;
        public static ServerEntry EditEntry { get; set; } = null;
        public static int SelectedSafe { get; set; } = -1;
        public static string Status { get; set; } = "";

        #endregion

        protected string ServerName { get; set; } = "Local";
        protected string ServerAddress { get; set; } = "127.0.0.1";
        protected string ServerPort { get; set; } = "6702";

        protected const float WindowHeight = 400;
        protected const float WindowWidth = 400;

        #endregion

        #region Base overrides

        public override bool Display => !Closed && HighLogic.LoadedScene == GameScenes.MAINMENU && SettingsSystem.CurrentSettings.DisclaimerAccepted;

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.9f - WindowWidth, Screen.height / 2f - WindowHeight / 2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            WindowStyle = new GUIStyle(GUI.skin.window);
            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            //ButtonStyle.fontSize = 10;
            StatusStyle = new GUIStyle(GUI.skin.label) { normal = { textColor = Color.yellow } };
            //StatusStyle.fontSize = 10;

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            LabelOptions = new GUILayoutOption[1];
            LabelOptions[0] = GUILayout.Width(100);
        }

        public override void OnGui()
        {
            base.OnGui();

            if (Display)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6702 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, $"LunaMultiPlayer {VersionInfo.VersionNumber}", WindowStyle, LayoutOptions));
        }

        public override void Update()
        {
            Status = SystemsContainer.Get<MainSystem>().Status;
            SelectedSafe = Selected;
            AddingServerSafe = AddingServer;
        }

        #endregion
    }
}
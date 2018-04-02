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

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.NetworkState >= ClientState.Running && HighLogic.LoadedScene >= GameScenes.SPACECENTER && SettingsSystem.ServerSettings.AllowAdmin;
            set => base.Display = _display = value;
        }

        #endregion

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6723 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.AdminWindowText.Title, WindowStyle, LayoutOptions));
            }
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

            ScrollPos = new Vector2();
        }
    }
}

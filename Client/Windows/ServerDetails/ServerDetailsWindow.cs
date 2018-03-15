using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.ServerDetails
{
    public partial class ServerDetailsWindow : Window<ServerDetailsWindow>
    {
        private const float WindowHeight = 50;
        private const float WindowWidth = 350;
        public long ServerId { get; set; }
        public string Password { get; set; }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6717 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, LocalizationContainer.ServerDetailsWindowText.Title, WindowStyle, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/4f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.Width(WindowWidth);
            LayoutOptions[1] = GUILayout.Height(WindowHeight);
            LayoutOptions[2] = GUILayout.ExpandWidth(true);
            LayoutOptions[3] = GUILayout.ExpandHeight(true);
        }

        public override void Update()
        {
            SafeDisplay = Display;
        }
    }
}
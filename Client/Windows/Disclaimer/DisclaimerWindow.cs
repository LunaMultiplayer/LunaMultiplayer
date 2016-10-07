using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Disclaimer
{
    public partial class DisclaimerWindow : Window<DisclaimerWindow>
    {
        private float WindowHeight { get; } = 300;
        private float WindowWidth { get; } = 500;

        public override bool Display => !SettingsSystem.CurrentSettings.DisclaimerAccepted;

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6713 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "LunaMultiPlayer - Disclaimer", LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            LayoutOptions = new GUILayoutOption[2];
            LayoutOptions[0] = GUILayout.ExpandWidth(true);
            LayoutOptions[1] = GUILayout.ExpandHeight(true);
        }
    }
}
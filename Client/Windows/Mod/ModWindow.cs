using LunaClient.Base;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.Mod
{
    public partial class ModWindow : Window<ModWindow>
    {
        private const float WindowHeight = 400;
        private const float WindowWidth = 600;

        public override void Update()
        {
            SafeDisplay = Display;
        }

        public override void OnGui()
        {
            base.OnGui();
            if (SafeDisplay)
                WindowRect =
                    LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6706 + MainSystem.WindowOffset, WindowRect,
                        DrawContent, "LunaMultiplayer - Mod Control", WindowStyle, LayoutOptions));
        }

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width/2f - WindowWidth/2f, Screen.height/2f - WindowHeight/2f, WindowWidth,
                WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);

            WindowStyle = new GUIStyle(GUI.skin.window);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            LabelStyle = new GUIStyle(GUI.skin.label);
            ScrollStyle = new GUIStyle(GUI.skin.scrollView);

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            ScrollPos = new Vector2();
        }
    }
}
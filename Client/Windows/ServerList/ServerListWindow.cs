using LunaClient.Base;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow : Window<ServerListWindow>
    {
        protected GUIStyle BigLabelStyle { get; set; }
        protected Vector2 ScrollPosition { get; set; }

        protected const float WindowHeight = 700;
        protected const float WindowWidth = 700;

        public override void SetStyles()
        {
            BigLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 80,
                normal = {textColor = Color.red}
            };

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

            ScrollStyle = new GUIStyle(GUI.skin.scrollView);
        }

        public override void OnGui()
        {
            base.OnGui();

            if (Display)
            {
                WindowRect = LmpGuiUtil.PreventOffscreenWindow(GUILayout.Window(6714 + MainSystem.WindowOffset, WindowRect,
                    DrawContent, "Server list", WindowStyle, LayoutOptions));
            }
        }
    }
}

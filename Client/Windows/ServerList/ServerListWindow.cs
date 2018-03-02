using LunaClient.Base;
using LunaClient.Network;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow : Window<ServerListWindow>
    {
        private static bool _display;
        public override bool Display
        {
            get => _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState == ClientState.Disconnected &&
                   HighLogic.LoadedScene < GameScenes.SPACECENTER;
            set => _display = value;
        }

        public IEnumerable<ServerInfo> DisplayedServers { get; set; } = NetworkServerList.Servers.Values;
        protected GUIStyle BigLabelStyle { get; set; }
        protected Vector2 VerticalScrollPosition { get; set; }
        protected Vector2 HorizontalScrollPosition { get; set; }

        protected float WindowHeight = Screen.height * 0.95f;
        protected float WindowWidth = Screen.width * 0.95f;

        private Texture2D KeyIcon { get; set; }

        public override void SetStyles()
        {
            BigLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 80,
                normal = { textColor = Color.red }
            };

            KeyIcon = WindowUtil.LoadIcon(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiPlayer", "Icons", "key.png"), 16, 16);

            WindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            WindowStyle = new GUIStyle(GUI.skin.window);

            BoxStyle = new GUIStyle(GUI.skin.box);
            var backgroundColor = new Color(0.145f, 0.165f, 0.198f, 0.8f);
            BoxStyle.normal.background = new Texture2D(1,1);
            BoxStyle.normal.background.SetPixel(0, 0, backgroundColor);
            BoxStyle.normal.background.Apply();
            BoxStyle.onNormal.background = new Texture2D(1, 1);
            BoxStyle.onNormal.background.SetPixel(0, 0, backgroundColor);
            BoxStyle.onNormal.background.Apply();

            TextAreaStyle = new GUIStyle(GUI.skin.textArea);
            ButtonStyle = new GUIStyle(GUI.skin.button);
            LabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

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

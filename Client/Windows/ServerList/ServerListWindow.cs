using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Network;
using LunaCommon;
using LunaCommon.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow : Window<ServerListWindow>
    {
        #region Fields

        private static readonly Dictionary<string, PropertyInfo> OrderByPropertyDictionary = new Dictionary<string, PropertyInfo>();

        protected float WindowHeight = Screen.height * 0.95f;
        protected float WindowWidth = Screen.width * 0.95f;
        protected float ServerDetailWindowHeight = 50;
        protected float ServerDetailWindowWidth = 350;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState == ClientState.Disconnected && HighLogic.LoadedScene == GameScenes.MAINMENU;
            set => base.Display = _display = value;
        }

        private static readonly List<ServerInfo> DisplayedServers = new List<ServerInfo>();
        private static Vector2 _verticalScrollPosition;
        private static Vector2 _horizontalScrollPosition;
        private static Rect _serverDetailWindowRect;
        private static GUILayoutOption[] _serverDetailLayoutOptions;

        private static long _selectedServerId;
        private static string _orderBy = "Ping";
        private static bool _ascending = true;

        #endregion

        #region Constructor

        public ServerListWindow()
        {
            foreach (var property in typeof(ServerInfo).GetProperties())
            {
                OrderByPropertyDictionary.Add(property.Name, property);
            }
        }

        #endregion

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            _serverDetailWindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, 10000, 20);
            
            BoxStyle = new GUIStyle(GUI.skin.box);
            var backgroundColor = new Color(0.145f, 0.165f, 0.198f, 0.8f);
            BoxStyle.normal.background = new Texture2D(1,1);
            BoxStyle.normal.background.SetPixel(0, 0, backgroundColor);
            BoxStyle.normal.background.Apply();
            BoxStyle.onNormal.background = new Texture2D(1, 1);
            BoxStyle.onNormal.background.SetPixel(0, 0, backgroundColor);
            BoxStyle.onNormal.background.Apply();
            
            LabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };

            LayoutOptions = new GUILayoutOption[4];
            LayoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            LayoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            LayoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            LayoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            _serverDetailLayoutOptions = new GUILayoutOption[4];
            _serverDetailLayoutOptions[0] = GUILayout.MinWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[1] = GUILayout.MaxWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[2] = GUILayout.MinHeight(ServerDetailWindowHeight);
            _serverDetailLayoutOptions[3] = GUILayout.MaxHeight(ServerDetailWindowHeight);

            LabelOptions = new GUILayoutOption[1];
            LabelOptions[0] = GUILayout.Width(100);
        }

        public override void OnGui()
        {
            base.OnGui();
            if (Display)
            {
                WindowRect = FixWindowPos(GUILayout.Window(6714 + MainSystem.WindowOffset, WindowRect, DrawContent, "Server list", WindowStyle, LayoutOptions));
                if (_selectedServerId != 0)
                {
                    _serverDetailWindowRect = FixWindowPos(GUILayout.Window(6715 + MainSystem.WindowOffset,
                        _serverDetailWindowRect, DrawServerDetailsContent, LocalizationContainer.ServerListWindowText.ServerDetailTitle, WindowStyle, _serverDetailLayoutOptions));
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Display)
            {
                DisplayedServers.Clear();
                DisplayedServers.AddRange(_ascending ? NetworkServerList.Servers.Values.OrderBy(s => OrderByPropertyDictionary[_orderBy].GetValue(s, null)) : 
                    NetworkServerList.Servers.Values.OrderByDescending(s =>OrderByPropertyDictionary[_orderBy].GetValue(s, null)));
            }
        }
    }
}

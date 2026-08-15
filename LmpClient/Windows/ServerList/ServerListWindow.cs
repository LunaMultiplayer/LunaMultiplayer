using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Network;
using LmpCommon;
using LmpCommon.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LmpClient.Windows.ServerList
{
    public partial class ServerListWindow : Window<ServerListWindow>
    {
        #region Fields

        /// <summary>
        /// Strongly-typed comparers keyed by the same column identifiers used by the grid header.
        /// This avoids the reflection + boxing that a per-frame property lookup would incur.
        /// </summary>
        private static readonly Dictionary<string, Comparison<ServerInfo>> OrderByComparers = new Dictionary<string, Comparison<ServerInfo>>
        {
            ["Password"] = (a, b) => a.Password.CompareTo(b.Password),
            ["Country"] = (a, b) => string.Compare(a.Country, b.Country, StringComparison.OrdinalIgnoreCase),
            ["Dedicated"] = (a, b) => a.DedicatedServer.CompareTo(b.DedicatedServer),
            ["Ping"] = (a, b) => a.Ping.CompareTo(b.Ping),
            ["Ping6"] = (a, b) => a.Ping6.CompareTo(b.Ping6),
            ["PlayerCount"] = (a, b) => a.PlayerCount.CompareTo(b.PlayerCount),
            ["MaxPlayers"] = (a, b) => a.MaxPlayers.CompareTo(b.MaxPlayers),
            ["GameMode"] = (a, b) => a.GameMode.CompareTo(b.GameMode),
            ["WarpMode"] = (a, b) => a.WarpMode.CompareTo(b.WarpMode),
            ["TerrainQuality"] = (a, b) => a.TerrainQuality.CompareTo(b.TerrainQuality),
            ["Cheats"] = (a, b) => a.Cheats.CompareTo(b.Cheats),
            ["ServerName"] = (a, b) => string.Compare(a.ServerName, b.ServerName, StringComparison.OrdinalIgnoreCase),
            ["WebsiteText"] = (a, b) => string.Compare(a.WebsiteText, b.WebsiteText, StringComparison.OrdinalIgnoreCase),
            ["Description"] = (a, b) => string.Compare(a.Description, b.Description, StringComparison.OrdinalIgnoreCase),
        };

        protected float WindowHeight = Screen.height * 0.95f;
        protected float WindowWidth = Screen.width * 0.95f;
        protected float ServerDetailWindowHeight = 50;
        protected float ServerDetailWindowWidth = 350;

        private static bool _display;
        public override bool Display
        {
            get => base.Display && _display && MainSystem.ToolbarShowGui && MainSystem.NetworkState == ClientState.Disconnected && HighLogic.LoadedScene == GameScenes.MAINMENU;
            set
            {
                if (!_display && value)
                    NetworkServerList.RequestServers();
                base.Display = _display = value;
            }
        }

        private static readonly List<ServerInfo> DisplayedServers = new List<ServerInfo>();
        private static Vector2 _verticalScrollPosition;
        private static Vector2 _horizontalScrollPosition;
        private static Rect _serverDetailWindowRect;
        private static GUILayoutOption[] _serverDetailLayoutOptions;

        private static long _selectedServerId;
        private static string _orderBy = "PlayerCount";
        private static bool _ascending;

        private static int _lastServersVersion = int.MinValue;
        private static bool _forceRebuild;

        private static GUIStyle _headerServerLine;
        private static GUIStyle _evenServerLine;
        private static GUIStyle _oddServerLine;
        private static GUIStyle _labelStyle;
        private static GUIStyle _kspLabelStyle;

        protected override bool Resizable => true;

        #endregion

        public override void SetStyles()
        {
            WindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            _serverDetailWindowRect = new Rect(Screen.width * 0.025f, Screen.height * 0.025f, WindowWidth, WindowHeight);
            MoveRect = new Rect(0, 0, int.MaxValue, TitleHeight);

            _headerServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _headerServerLine.normal.background.SetPixel(0, 0, new Color(0.04f, 0.04f, 0.04f, 0.9f));
            _headerServerLine.normal.background.Apply();
            _headerServerLine.onNormal.background = new Texture2D(1, 1);
            _headerServerLine.onNormal.background.SetPixel(0, 0, new Color(0.04f, 0.04f, 0.04f, 0.9f));
            _headerServerLine.onNormal.background.Apply();

            _evenServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _evenServerLine.normal.background.SetPixel(0, 0, new Color(0.120f, 0.120f, 0.150f, 0.9f));
            _evenServerLine.normal.background.Apply();
            _evenServerLine.onNormal.background = new Texture2D(1, 1);
            _evenServerLine.onNormal.background.SetPixel(0, 0, new Color(0.120f, 0.120f, 0.150f, 0.9f));
            _evenServerLine.onNormal.background.Apply();

            _oddServerLine = new GUIStyle
            {
                normal =
                {
                    background = new Texture2D(1, 1)
                }
            };
            _oddServerLine.normal.background.SetPixel(0, 0, new Color(0.180f, 0.180f, 0.220f, 0.9f));
            _oddServerLine.normal.background.Apply();
            _oddServerLine.onNormal.background = new Texture2D(1, 1);
            _oddServerLine.onNormal.background.SetPixel(0, 0, new Color(0.180f, 0.180f, 0.220f, 0.9f));
            _oddServerLine.onNormal.background.Apply();

            _kspLabelStyle = new GUIStyle(Skin.label) { alignment = TextAnchor.MiddleCenter };
            _labelStyle = new GUIStyle(Skin.label) { alignment = TextAnchor.MiddleCenter, normal = GUI.skin.label.normal };

            _serverDetailLayoutOptions = new GUILayoutOption[4];
            _serverDetailLayoutOptions[0] = GUILayout.MinWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[1] = GUILayout.MaxWidth(ServerDetailWindowWidth);
            _serverDetailLayoutOptions[2] = GUILayout.MinHeight(ServerDetailWindowHeight);
            _serverDetailLayoutOptions[3] = GUILayout.MaxHeight(ServerDetailWindowHeight);

            LabelOptions = new GUILayoutOption[1];
            LabelOptions[0] = GUILayout.Width(100);
        }

        protected override void DrawGui()
        {
            WindowRect = FixWindowPos(GUILayout.Window(6714 + MainSystem.WindowOffset, WindowRect, DrawContent, LocalizationContainer.ServerListWindowText.Title));
            if (_selectedServerId != 0)
            {
                _serverDetailWindowRect = FixWindowPos(GUILayout.Window(6715 + MainSystem.WindowOffset,
                    _serverDetailWindowRect, DrawServerDetailsContent, LocalizationContainer.ServerListWindowText.ServerDetailTitle, _serverDetailLayoutOptions));
            }
        }

        public override void Update()
        {
            base.Update();
            if (!Display)
                return;

            //Only rebuild the displayed list when the underlying data or the sort/filter selection
            //actually changed. Otherwise we'd re-sort and re-filter the whole list every single frame.
            var version = NetworkServerList.ServersVersion;
            if (!_forceRebuild && version == _lastServersVersion)
                return;

            _lastServersVersion = version;
            _forceRebuild = false;
            RebuildDisplayedServers();
        }

        private static void RebuildDisplayedServers()
        {
            DisplayedServers.Clear();

            foreach (var server in NetworkServerList.Servers.Values)
            {
                if (ServerFilter.MatchesFilters(server))
                    DisplayedServers.Add(server);
            }

            if (OrderByComparers.TryGetValue(_orderBy, out var comparison))
            {
                DisplayedServers.Sort(comparison);
                if (!_ascending)
                    DisplayedServers.Reverse();
            }
        }

        /// <summary>
        /// Forces the displayed server list to be rebuilt on the next update. Call this whenever the
        /// sort column, sort direction or an active filter changes.
        /// </summary>
        public static void MarkDirty() => _forceRebuild = true;

        private static void SetOrderBy(string orderBy)
        {
            _orderBy = orderBy;
            _forceRebuild = true;
        }

        private static GUIStyle GetCorrectLabelStyle(ServerInfo server)
        {
            return server.DedicatedServer ? _labelStyle : _kspLabelStyle;
        }

        private static GUIStyle GetCorrectHyperlinkLabelStyle(ServerInfo server)
        {
            return server.DedicatedServer ? _labelStyle : HyperlinkLabelStyle;
        }
    }
}

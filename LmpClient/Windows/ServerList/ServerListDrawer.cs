using System.Linq;
using LmpClient.Localization;
using LmpClient.Network;
using LmpCommon;
using LmpCommon.Enums;
using UnityEngine;

namespace LmpClient.Windows.ServerList
{
    public partial class ServerListWindow
    {
        private static readonly float[] HeaderGridSize = new float[13];

        #region Servers grid

        protected override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(RefreshBigIcon))
            {
                NetworkServerList.RequestServers();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            DrawServersGrid();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawServersGrid()
        {
            GUILayout.BeginHorizontal();
            _verticalScrollPosition = GUILayout.BeginScrollView(_verticalScrollPosition);
            GUILayout.BeginVertical();
            _horizontalScrollPosition = GUILayout.BeginScrollView(_horizontalScrollPosition);
            DrawGridHeader();
            DrawServerList();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        private void DrawGridHeader()
        {
            GUILayout.BeginHorizontal(_headerServerLine);

            GUILayout.BeginHorizontal(GUILayout.Width(25));
            if (GUILayout.Button(_ascending ? "▲" : "▼"))
            {
                _ascending = !_ascending;
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[0] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(30));
            if (GUILayout.Button(KeyIcon))
            {
                _orderBy = "Password";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[1] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(30));
            if (GUILayout.Button(GlobeIcon))
            {
                _orderBy = "Country";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[2] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Ping))
            {
                _orderBy = "Ping";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[3] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Players))
            {
                _orderBy = "PlayerCount";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[4] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.MaxPlayers))
            {
                _orderBy = "MaxPlayers";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[5] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Mode))
            {
                _orderBy = "GameMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[6] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(75));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.WarpMode))
            {
                _orderBy = "WarpMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[7] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Terrain))
            {
                _orderBy = "TerrainQuality";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[8] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Cheats))
            {
                _orderBy = "Cheats";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[9] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(220));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Name))
            {
                _orderBy = "ServerName";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[10] = GUILayoutUtility.GetLastRect().width > 220 ? GUILayoutUtility.GetLastRect().width : 220;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(150));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Website))
            {
                _orderBy = "WebsiteText";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[11] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(600));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Description))
            {
                _orderBy = "Description";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[12] = GUILayoutUtility.GetLastRect().width > 600 ? GUILayoutUtility.GetLastRect().width : 600;
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private void DrawServerList()
        {
            GUILayout.BeginHorizontal();

            if (DisplayedServers == null || !DisplayedServers.Any())
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                GUILayout.Label(LocalizationContainer.ServerListWindowText.NoServers, BigLabelStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginVertical();

                for (var i = 0; i < DisplayedServers.Count; i++)
                {
                    var currentEntry = DisplayedServers[i];

                    GUILayout.BeginHorizontal(i % 2 != 0 ? _oddServerLine : _evenServerLine);
                    DrawServerEntry(currentEntry);
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawServerEntry(ServerInfo currentEntry)
        {
            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[0]));
            if (GUILayout.Button("▶"))
            {
                if (currentEntry.Password)
                {
                    _selectedServerId = currentEntry.Id;
                }
                else
                {
                    NetworkServerList.IntroduceToServer(currentEntry.Id);
                    Display = false;
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[1]));
            if (currentEntry.Password)
                GUILayout.Label(KeyIcon, GUILayout.MinWidth(HeaderGridSize[1]));
            else
                GUILayout.Label("", GUILayout.MinWidth(HeaderGridSize[1]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[2]));
            GUILayout.Label(new GUIContent($"{currentEntry.Country}"), GUILayout.MinWidth(HeaderGridSize[2]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[3]));
            GUILayout.Label(new GUIContent($"{currentEntry.DisplayedPing}"), GUILayout.MinWidth(HeaderGridSize[3]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[4]));
            GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}"), GUILayout.MinWidth(HeaderGridSize[4]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[5]));
            GUILayout.Label(new GUIContent($"{currentEntry.MaxPlayers}"), GUILayout.MinWidth(HeaderGridSize[5]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[6]));
            GUILayout.Label(new GUIContent($"{(GameMode)currentEntry.GameMode}"), GUILayout.MinWidth(HeaderGridSize[6]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[7]));
            GUILayout.Label(new GUIContent($"{(WarpMode)currentEntry.WarpMode}"), GUILayout.MinWidth(HeaderGridSize[7]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[8]));
            GUILayout.Label(new GUIContent($"{(TerrainQuality)currentEntry.TerrainQuality}"), GUILayout.MinWidth(HeaderGridSize[8]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[9]));
            GUILayout.Label(new GUIContent($"{currentEntry.Cheats}"), GUILayout.MinWidth(HeaderGridSize[9]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[10]));
            GUILayout.Label(new GUIContent($"{currentEntry.ServerName}"), GUILayout.MinWidth(HeaderGridSize[10]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[11]));
            if (!string.IsNullOrEmpty(currentEntry.Website))
            {
                if (GUILayout.Button(new GUIContent(currentEntry.WebsiteText), HyperlinkLabelStyle, GUILayout.MinWidth(HeaderGridSize[11])))
                {
                    Application.OpenURL(currentEntry.Website);
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[12]));
            GUILayout.Label(new GUIContent($"{currentEntry.Description}"), GUILayout.MinWidth(HeaderGridSize[12]));
            GUILayout.EndHorizontal();
        }

        #endregion

        #region Server details dialog

        public void DrawServerDetailsContent(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _selectedServerId = 0, _serverDetailWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ServerListWindowText.Password, LabelOptions);
            NetworkServerList.Password = GUILayout.PasswordField(NetworkServerList.Password, '*', 30, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Connect))
            {
                NetworkServerList.IntroduceToServer(_selectedServerId);
                _selectedServerId = 0;
                Display = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}

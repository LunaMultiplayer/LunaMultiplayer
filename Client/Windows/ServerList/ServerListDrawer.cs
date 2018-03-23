using LunaClient.Localization;
using LunaClient.Network;
using LunaCommon;
using LunaCommon.Enums;
using System.Linq;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow
    {
        private static readonly float[] HeaderGridSize = new float[11];

        #region Servers grid

        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(RefreshBigIcon, ButtonStyle))
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
            _verticalScrollPosition = GUILayout.BeginScrollView(_verticalScrollPosition, ScrollStyle);
            GUILayout.BeginVertical();
            _horizontalScrollPosition = GUILayout.BeginScrollView(_horizontalScrollPosition, ScrollStyle);
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
            if (GUILayout.Button(_ascending ? "▲" : "▼", ButtonStyle))
            {
                _ascending = !_ascending;
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[0] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(30));
            if (GUILayout.Button(KeyIcon, ButtonStyle))
            {
                _orderBy = "Password";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[1] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Ping, ButtonStyle))
            {
                _orderBy = "Ping";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[2] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Players, ButtonStyle))
            {
                _orderBy = "PlayerCount";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[3] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.MaxPlayers, ButtonStyle))
            {
                _orderBy = "MaxPlayers";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[4] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Mode, ButtonStyle))
            {
                _orderBy = "GameMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[5] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(75));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.WarpMode, ButtonStyle))
            {
                _orderBy = "WarpMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[6] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Terrain, ButtonStyle))
            {
                _orderBy = "TerrainQuality";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[7] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Cheats, ButtonStyle))
            {
                _orderBy = "Cheats";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[8] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Name, ButtonStyle))
            {
                _orderBy = "ServerName";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[9] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(550));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Description, ButtonStyle))
            {
                _orderBy = "Description";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[10] = GUILayoutUtility.GetLastRect().width;
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
            if (GUILayout.Button("▶", ButtonStyle))
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
                GUILayout.Label(new GUIContent(KeyIcon, "Password"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[1]));
            else
                GUILayout.Label("", GUILayout.MinWidth(HeaderGridSize[1]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[2]));
            GUILayout.Label(new GUIContent($"{currentEntry.DisplayedPing}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[2]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[3]));
            GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[3]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[4]));
            GUILayout.Label(new GUIContent($"{currentEntry.MaxPlayers}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[4]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[5]));
            GUILayout.Label(new GUIContent($"{(GameMode) currentEntry.GameMode}"), LabelStyle,
                GUILayout.MinWidth(HeaderGridSize[5]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[6]));
            GUILayout.Label(new GUIContent($"{(WarpMode) currentEntry.WarpMode}"), LabelStyle,
                GUILayout.MinWidth(HeaderGridSize[6]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[7]));
            GUILayout.Label(new GUIContent($"{(TerrainQuality) currentEntry.TerrainQuality}"), LabelStyle,
                GUILayout.MinWidth(HeaderGridSize[7]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[8]));
            GUILayout.Label(new GUIContent($"{currentEntry.Cheats}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[8]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
            GUILayout.Label(new GUIContent($"{currentEntry.ServerName}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[9]));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(550));
            GUILayout.Label(new GUIContent($"{currentEntry.Description}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[10]));
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
            NetworkServerList.Password = GUILayout.TextArea(NetworkServerList.Password, 30, TextAreaStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Connect, ButtonStyle))
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

using LunaClient.Localization;
using LunaClient.Network;
using LunaCommon.Enums;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow
    {
        private static bool Ascending { get; set; } = true;

        private float[] HeaderGridSize = new float[10];

        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Close, ButtonStyle))
                Display = false;
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Refresh, ButtonStyle))
            {
                NetworkServerList.RequestServers();
                Thread.Sleep(500);
                DisplayedServers = NetworkServerList.Servers.Values;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(WindowWidth));
            DrawServersGrid();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawServersGrid()
        {
            GUILayout.BeginHorizontal();
            VerticalScrollPosition = GUILayout.BeginScrollView(VerticalScrollPosition, ScrollStyle);
            GUILayout.BeginVertical();
            HorizontalScrollPosition = GUILayout.BeginScrollView(HorizontalScrollPosition, ScrollStyle);
            DrawGridHeader();
            DrawServerList();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndHorizontal();
        }

        private void DrawServerList()
        {
            GUILayout.BeginHorizontal();

            if (DisplayedServers == null || !DisplayedServers.Any())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(WindowWidth * 0.25f);
                GUILayout.BeginVertical();
                GUILayout.Space(WindowHeight * 0.25f);
                GUILayout.Label(LocalizationContainer.ServerListWindowText.NoServers, BigLabelStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginVertical();
                foreach (var currentEntry in DisplayedServers)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[0]));
                    if (GUILayout.Button("▶", ButtonStyle))
                    {
                        NetworkServerList.IntroduceToServer(currentEntry.Id);
                        Display = false;
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[1]));
                    GUILayout.Label(new GUIContent($"{currentEntry.Ping}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[1]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[2]));
                    GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[2]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[3]));
                    GUILayout.Label(new GUIContent($"{currentEntry.MaxPlayers}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[3]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[4]));
                    GUILayout.Label(new GUIContent($"{(GameMode)currentEntry.GameMode}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[4]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[5]));
                    GUILayout.Label(new GUIContent($"{(WarpMode)currentEntry.WarpMode}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[5]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[6]));
                    GUILayout.Label(new GUIContent($"{(TerrainQuality)currentEntry.TerrainQuality}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[6]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[7]));
                    GUILayout.Label(new GUIContent($"{currentEntry.Cheats}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[7]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
                    GUILayout.Label(new GUIContent($"{currentEntry.ServerName}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[8]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(1000));
                    GUILayout.Label(new GUIContent($"{currentEntry.Description}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[9]));
                    GUILayout.EndHorizontal();

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawGridHeader()
        {
            GUILayout.BeginHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(25));
            if (GUILayout.Button(Ascending ? "▲" : "▼", ButtonStyle))
            {
                Ascending = !Ascending;
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[0] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Ping, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Ping) : 
                    DisplayedServers.OrderByDescending(s => s.Ping);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[1] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Players, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.PlayerCount) :
                    DisplayedServers.OrderByDescending(s => s.PlayerCount);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[2] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.MaxPlayers, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.MaxPlayers) :
                    DisplayedServers.OrderByDescending(s => s.MaxPlayers);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[3] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Mode, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.GameMode) :
                    DisplayedServers.OrderByDescending(s => s.GameMode);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[4] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(75));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.WarpMode, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.WarpMode) :
                    DisplayedServers.OrderByDescending(s => s.WarpMode);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[5] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Terrain, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.TerrainQuality) :
                    DisplayedServers.OrderByDescending(s => s.TerrainQuality);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[6] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Cheats, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Cheats) :
                    DisplayedServers.OrderByDescending(s => s.Cheats);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[7] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Name, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.ServerName) :
                    DisplayedServers.OrderByDescending(s => s.ServerName);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[8] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(1000));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Description, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Description) :
                    DisplayedServers.OrderByDescending(s => s.Description);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[9] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
    }
}

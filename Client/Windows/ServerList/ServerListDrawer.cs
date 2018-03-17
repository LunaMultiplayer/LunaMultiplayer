using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Windows.ServerDetails;
using LunaCommon.Enums;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow
    {
        private static bool Ascending { get; set; } = true;

        private static readonly float[] HeaderGridSize = new float[11];

        public override void DrawWindowContent(int windowId)
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

            GUILayout.BeginHorizontal();
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

        private void DrawGridHeader()
        {
            GUILayout.BeginHorizontal(GUI.skin.box);

            GUILayout.BeginHorizontal(GUILayout.Width(25));
            if (GUILayout.Button(Ascending ? "▲" : "▼", ButtonStyle))
            {
                Ascending = !Ascending;
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[0] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(30));
            if (GUILayout.Button(KeyIcon, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Password) :
                    DisplayedServers.OrderByDescending(s => s.Password);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[1] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Ping, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Ping) :
                    DisplayedServers.OrderByDescending(s => s.Ping);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[2] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Players, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.PlayerCount) :
                    DisplayedServers.OrderByDescending(s => s.PlayerCount);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[3] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.MaxPlayers, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.MaxPlayers) :
                    DisplayedServers.OrderByDescending(s => s.MaxPlayers);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[4] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Mode, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.GameMode) :
                    DisplayedServers.OrderByDescending(s => s.GameMode);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[5] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(75));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.WarpMode, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.WarpMode) :
                    DisplayedServers.OrderByDescending(s => s.WarpMode);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[6] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Terrain, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.TerrainQuality) :
                    DisplayedServers.OrderByDescending(s => s.TerrainQuality);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[7] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Cheats, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Cheats) :
                    DisplayedServers.OrderByDescending(s => s.Cheats);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[8] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Name, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.ServerName) :
                    DisplayedServers.OrderByDescending(s => s.ServerName);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[9] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(550));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Description, ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Description) :
                    DisplayedServers.OrderByDescending(s => s.Description);
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[10] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }

        private void DrawServerList()
        {
            GUILayout.BeginHorizontal(BoxStyle);

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
                foreach (var currentEntry in DisplayedServers)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[0]));
                    if (GUILayout.Button("▶", ButtonStyle))
                    {
                        if (currentEntry.Password)
                        {
                            ServerDetailsWindow.Singleton.ServerId = currentEntry.Id;
                            ServerDetailsWindow.Singleton.Display = true;
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
                    GUILayout.Label(new GUIContent($"{currentEntry.Ping}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[2]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[3]));
                    GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[3]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[4]));
                    GUILayout.Label(new GUIContent($"{currentEntry.MaxPlayers}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[4]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[5]));
                    GUILayout.Label(new GUIContent($"{(GameMode)currentEntry.GameMode}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[5]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[6]));
                    GUILayout.Label(new GUIContent($"{(WarpMode)currentEntry.WarpMode}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[6]));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.MinWidth(HeaderGridSize[7]));
                    GUILayout.Label(new GUIContent($"{(TerrainQuality)currentEntry.TerrainQuality}"), LabelStyle, GUILayout.MinWidth(HeaderGridSize[7]));
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

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }
}

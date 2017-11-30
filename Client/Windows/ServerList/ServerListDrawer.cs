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

        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", ButtonStyle))
                Display = false;
            if (GUILayout.Button("Refresh", ButtonStyle))
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
                GUILayout.Label("No servers!", BigLabelStyle);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginVertical();
                foreach (var currentEntry in DisplayedServers)
                {
                    GUILayout.BeginHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(25));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("▶", ButtonStyle))
                    {
                        NetworkServerList.IntroduceToServer(currentEntry.Id);
                        Display = false;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(50));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{currentEntry.Ping}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(50));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(90));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{currentEntry.MaxPlayers}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(85));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{(GameMode)currentEntry.GameMode}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(85));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{(WarpMode)currentEntry.WarpMode}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(50));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{(TerrainQuality)currentEntry.TerrainQuality}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(35));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent($"{currentEntry.Cheats}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(325));
                    GUILayout.Space(20);
                    GUILayout.Label(new GUIContent($"{currentEntry.ServerName}"));
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(GUILayout.Width(1000));
                    GUILayout.Space(20);
                    GUILayout.Label(new GUIContent($"{currentEntry.Description}"));
                    GUILayout.FlexibleSpace();
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
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(50));
            if (GUILayout.Button("Ping", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Ping) : 
                    DisplayedServers.OrderByDescending(s => s.Ping);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(50));
            if (GUILayout.Button("Players", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.PlayerCount) :
                    DisplayedServers.OrderByDescending(s => s.PlayerCount);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(85));
            if (GUILayout.Button("Max players", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.MaxPlayers) :
                    DisplayedServers.OrderByDescending(s => s.MaxPlayers);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(85));
            if (GUILayout.Button("Mode", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.GameMode) :
                    DisplayedServers.OrderByDescending(s => s.GameMode);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(75));
            if (GUILayout.Button("Warp mode", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.WarpMode) :
                    DisplayedServers.OrderByDescending(s => s.WarpMode);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(50));
            if (GUILayout.Button("Terrain", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.TerrainQuality) :
                    DisplayedServers.OrderByDescending(s => s.TerrainQuality);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(50));
            if (GUILayout.Button("Cheats", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Cheats) :
                    DisplayedServers.OrderByDescending(s => s.Cheats);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(325));
            if (GUILayout.Button("Name", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.ServerName) :
                    DisplayedServers.OrderByDescending(s => s.ServerName);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.Width(1000));
            if (GUILayout.Button("Description", ButtonStyle))
            {
                DisplayedServers = Ascending ? DisplayedServers.OrderBy(s => s.Description) :
                    DisplayedServers.OrderByDescending(s => s.Description);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndHorizontal();
        }
    }
}

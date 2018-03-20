using LunaClient.Localization;
using LunaClient.Network;
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
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(RefreshIcon, ButtonStyle))
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
                OrderBy = "Password";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[1] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Ping, ButtonStyle))
            {
                OrderBy = "Ping";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[2] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Players, ButtonStyle))
            {
                OrderBy = "PlayerCount";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[3] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.MaxPlayers, ButtonStyle))
            {
                OrderBy = "MaxPlayers";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[4] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(85));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Mode, ButtonStyle))
            {
                OrderBy = "GameMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[5] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(75));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.WarpMode, ButtonStyle))
            {
                OrderBy = "WarpMode";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[6] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Terrain, ButtonStyle))
            {
                OrderBy = "TerrainQuality";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[7] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(50));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Cheats, ButtonStyle))
            {
                OrderBy = "Cheats";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[8] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(325));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Name, ButtonStyle))
            {
                OrderBy = "ServerName";
            }
            if (Event.current.type == EventType.Repaint) HeaderGridSize[9] = GUILayoutUtility.GetLastRect().width;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(GUILayout.MinWidth(550));
            if (GUILayout.Button(LocalizationContainer.ServerListWindowText.Description, ButtonStyle))
            {
                OrderBy = "Description";
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
                            SelectedServerId = currentEntry.Id;
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

        #endregion

        #region Server details dialog

        public void DrawServerDetailsContent(int windowId)
        {            
            //Always draw close button first
            DrawCloseButton(() => SelectedServerId = 0, ServerDetailWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ServerDetailsWindowText.Password, LabelOptions);
            NetworkServerList.Password = GUILayout.TextArea(NetworkServerList.Password, 30, TextAreaStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(LocalizationContainer.ServerDetailsWindowText.Connect, ButtonStyle))
            {
                NetworkServerList.IntroduceToServer(SelectedServerId);
                SelectedServerId = 0;
                Display = false;
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        #endregion
    }
}

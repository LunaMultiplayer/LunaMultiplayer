using System;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaCommon.Enums;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.ServerList
{
    public partial class ServerListWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", ButtonStyle))
                Display = false;
            if (GUILayout.Button("Refresh", ButtonStyle))
                NetworkServerList.RequestServers();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Name");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Players/Max");
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Mode");
            GUILayout.EndVertical();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
            
            if (NetworkServerList.Servers == null || !NetworkServerList.Servers.Any())
            {
                GUILayout.Space(200);
                GUILayout.BeginHorizontal();
                GUILayout.Label("No servers!", BigLabelStyle);
                GUILayout.EndHorizontal();
            }
            else
            {
                ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, ScrollStyle);
                foreach (var currentEntry in NetworkServerList.Servers)
                {
                    GUILayout.BeginHorizontal();

                    var tooltip = currentEntry.Description;
                    GUILayout.BeginVertical();
                    GUILayout.Label(new GUIContent($"{currentEntry.ServerName}", tooltip));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label(new GUIContent($"{currentEntry.PlayerCount}/{currentEntry.MaxPlayers}", tooltip));
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical();
                    GUILayout.Label(new GUIContent($"{(GameMode)currentEntry.GameMode}", tooltip));
                    GUILayout.EndVertical();

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Connect", ButtonStyle))
                    {
                        NetworkServerList.IntroduceToServer(currentEntry.Id);
                        Display = false;
                    }

                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.EndVertical();
        }
    }
}

using LunaClient.Network;
using LunaClient.Systems.Network;
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
            if (NetworkServerList.Servers == null || !NetworkServerList.Servers.Any())
                GUILayout.Label("No servers!");
            else
            {
                foreach (var currentEntry in NetworkServerList.Servers)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{currentEntry.ServerName}---{currentEntry.PlayerCount}/{currentEntry.MaxPlayers}");
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Connect", ButtonStyle))
                    {
                        NetworkServerList.IntroduceToServer(currentEntry.Id);
                        Display = false;
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }
    }
}

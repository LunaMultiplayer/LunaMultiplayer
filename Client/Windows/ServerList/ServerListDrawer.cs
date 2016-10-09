using LunaClient.Systems.Network;
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
                NetworkSystem.Singleton.RequestServers();

            foreach (var currentEntry in NetworkSystem.Singleton.Servers)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{currentEntry.ServerName}---{currentEntry.PlayerCount}/{currentEntry.MaxPlayers}");
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Connect", ButtonStyle))
                    NetworkSystem.Singleton.IntroduceToServer(currentEntry.Id);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}

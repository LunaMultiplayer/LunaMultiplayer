using LmpClient.Localization;
using LmpCommon;
using UnityEngine;

namespace LmpClient.Windows.ServerList
{
    public class ServerFilter
    {
        public static bool HidePrivateServers = false;
        public static bool HideFullServers = true;
        public static bool HideEmptyServers = false;
        public static bool DedicatedServersOnly = false;

        public static void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            HideFullServers = GUILayout.Toggle(HideFullServers, LocalizationContainer.ServerListFiltersText.HideFullServers);
            GUILayout.FlexibleSpace();
            HideEmptyServers = GUILayout.Toggle(HideEmptyServers, LocalizationContainer.ServerListFiltersText.HideEmptyServers);
            GUILayout.FlexibleSpace();
            HidePrivateServers = GUILayout.Toggle(HidePrivateServers, LocalizationContainer.ServerListFiltersText.HidePrivateServers);
            GUILayout.FlexibleSpace();
            DedicatedServersOnly = GUILayout.Toggle(DedicatedServersOnly, LocalizationContainer.ServerListFiltersText.DedicatedServersOnly);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static bool MatchesFilters(ServerInfo server)
        {
            if (HidePrivateServers && server.Password)
                return false;

            if (HideFullServers && server.PlayerCount == server.MaxPlayers)
                return false;

            if (HideEmptyServers && server.PlayerCount == 0)
                return false;

            if (DedicatedServersOnly && !server.DedicatedServer)
                return false;

            return true;
        }
    }
}

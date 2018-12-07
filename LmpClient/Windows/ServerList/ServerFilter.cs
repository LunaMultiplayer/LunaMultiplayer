using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using LmpCommon;
using UnityEngine;

namespace LmpClient.Windows.ServerList
{
    public class ServerFilter
    {
        public static void DrawFilters()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var hideFullServers = GUILayout.Toggle(SettingsSystem.CurrentSettings.ServerFilters.HideFullServers, LocalizationContainer.ServerListFiltersText.HideFullServers);
            if (hideFullServers != SettingsSystem.CurrentSettings.ServerFilters.HideFullServers)
            {
                SettingsSystem.CurrentSettings.ServerFilters.HideFullServers = hideFullServers;
                SettingsSystem.SaveSettings();
            }
            GUILayout.FlexibleSpace();
            var hideEmptyServers = GUILayout.Toggle(SettingsSystem.CurrentSettings.ServerFilters.HideEmptyServers, LocalizationContainer.ServerListFiltersText.HideEmptyServers);
            if (hideEmptyServers != SettingsSystem.CurrentSettings.ServerFilters.HideEmptyServers)
            {
                SettingsSystem.CurrentSettings.ServerFilters.HideEmptyServers = hideEmptyServers;
                SettingsSystem.SaveSettings();
            }
            GUILayout.FlexibleSpace();
            var hidePrivateServers = GUILayout.Toggle(SettingsSystem.CurrentSettings.ServerFilters.HidePrivateServers, LocalizationContainer.ServerListFiltersText.HidePrivateServers);
            if (hidePrivateServers != SettingsSystem.CurrentSettings.ServerFilters.HidePrivateServers)
            {
                SettingsSystem.CurrentSettings.ServerFilters.HidePrivateServers = hidePrivateServers;
                SettingsSystem.SaveSettings();
            }
            GUILayout.FlexibleSpace();
            var dedicatedServersOnly = GUILayout.Toggle(SettingsSystem.CurrentSettings.ServerFilters.DedicatedServersOnly, LocalizationContainer.ServerListFiltersText.DedicatedServersOnly);
            if (dedicatedServersOnly != SettingsSystem.CurrentSettings.ServerFilters.DedicatedServersOnly)
            {
                SettingsSystem.CurrentSettings.ServerFilters.DedicatedServersOnly = dedicatedServersOnly;
                SettingsSystem.SaveSettings();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static bool MatchesFilters(ServerInfo server)
        {
            if (SettingsSystem.CurrentSettings.ServerFilters.HidePrivateServers && server.Password)
                return false;

            if (SettingsSystem.CurrentSettings.ServerFilters.HideFullServers && server.PlayerCount == server.MaxPlayers)
                return false;

            if (SettingsSystem.CurrentSettings.ServerFilters.HideEmptyServers && server.PlayerCount == 0)
                return false;

            if (SettingsSystem.CurrentSettings.ServerFilters.DedicatedServersOnly && !server.DedicatedServer)
                return false;

            return true;
        }
    }
}

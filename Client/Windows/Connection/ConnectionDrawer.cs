using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems.SettingsSys;
using LunaClient.Windows.Options;
using LunaClient.Windows.ServerList;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Connection
{
    public partial class ConnectionWindow
    {
        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Space(10);

            DrawPlayerNameSection();
            DrawTopButtons();
            DrawCustomServers();
            
            GUILayout.Label(MainSystem.Singleton.Status, StatusStyle);
            GUILayout.EndVertical();
        }

        private void DrawCustomServers()
        {
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.CustomServers);
            GUILayout.BeginVertical(BoxStyle);

            if (GUILayout.Button(PlusIcon, ButtonStyle))
            {
                SettingsSystem.CurrentSettings.Servers.Insert(0, new ServerEntry());
                SettingsSystem.SaveSettings();
            }
            GUILayout.Space(15);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5),
                GUILayout.Height(WindowHeight - 100));
            for (var serverPos = 0; serverPos < SettingsSystem.CurrentSettings.Servers.Count; serverPos++)
            {
                GUILayout.BeginHorizontal();
                var selected = GUILayout.Toggle(SelectedIndex == serverPos,
                    SettingsSystem.CurrentSettings.Servers[serverPos].Name, ButtonStyle);
                if (GUILayout.Button(DeleteIcon, ButtonStyle, GUILayout.Width(35)))
                {
                    SettingsSystem.CurrentSettings.Servers.RemoveAt(SelectedIndex);
                    SettingsSystem.SaveSettings();
                }
                else
                {
                    GUILayout.EndHorizontal();
                    if (selected)
                    {
                        DrawServerEntry(serverPos);
                    }
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawServerEntry(int serverPos)
        {
            SelectedIndex = serverPos;
            GUILayout.BeginVertical(BoxStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.Name, LabelOptions);
            var newServerName = GUILayout.TextArea(SettingsSystem.CurrentSettings.Servers[serverPos].Name, TextAreaStyle);
            if (newServerName != SettingsSystem.CurrentSettings.Servers[serverPos].Name)
            {
                SettingsSystem.CurrentSettings.Servers[serverPos].Name = newServerName;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.Address, LabelOptions);
            var newAddress = GUILayout.TextArea(SettingsSystem.CurrentSettings.Servers[serverPos].Address, TextAreaStyle);
            if (newAddress != SettingsSystem.CurrentSettings.Servers[serverPos].Address)
            {
                SettingsSystem.CurrentSettings.Servers[serverPos].Address = newAddress;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.Port, LabelOptions);
            var port = GUILayout.TextArea(SettingsSystem.CurrentSettings.Servers[serverPos].Port.ToString(), TextAreaStyle);
            if (int.TryParse(port, out var parsedPort))
            {
                if (parsedPort != SettingsSystem.CurrentSettings.Servers[serverPos].Port)
                {
                    SettingsSystem.CurrentSettings.Servers[serverPos].Port = parsedPort;
                    SettingsSystem.SaveSettings();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.Password, LabelOptions);
            var newPassword = GUILayout.TextArea(SettingsSystem.CurrentSettings.Servers[serverPos].Password, TextAreaStyle);
            if (newPassword != SettingsSystem.CurrentSettings.Servers[serverPos].Password)
            {
                SettingsSystem.CurrentSettings.Servers[serverPos].Password = newPassword;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawTopButtons()
        {
            GUILayout.BeginHorizontal();
            if (MainSystem.NetworkState <= ClientState.Disconnected)
            {
                GUI.enabled = SettingsSystem.CurrentSettings.Servers.Count > SelectedIndex && SelectedIndex >= 0;
                if (GUILayout.Button(ConnectBigIcon, ButtonStyle))
                {
                    NetworkConnection.ConnectToServer(
                        SettingsSystem.CurrentSettings.Servers[SelectedIndex].Address,
                        SettingsSystem.CurrentSettings.Servers[SelectedIndex].Port,
                        SettingsSystem.CurrentSettings.Servers[SelectedIndex].Password);
                }
            }
            else
            {
                if (GUILayout.Button(DisconnectBigIcon, ButtonStyle))
                {
                    NetworkConnection.Disconnect("Cancelled connection to server");
                }
            }

            GUI.enabled = true;
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, SettingsBigIcon, ButtonStyle);
            ServerListWindow.Singleton.Display = GUILayout.Toggle(ServerListWindow.Singleton.Display, ServerBigIcon, ButtonStyle);
            GUILayout.EndHorizontal();
        }

        private void DrawPlayerNameSection()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.PlayerName, LabelOptions);
            GUI.enabled = MainSystem.NetworkState <= ClientState.Disconnected;
            var newPlayerName = GUILayout.TextArea(SettingsSystem.CurrentSettings.PlayerName, 32, TextAreaStyle); // Max 32 characters
            if (newPlayerName != SettingsSystem.CurrentSettings.PlayerName)
            {
                SettingsSystem.CurrentSettings.PlayerName = newPlayerName.Trim().Replace("\n", "");
                SettingsSystem.SaveSettings();
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }
    }
}

using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.Windows.Options;
using LunaClient.Windows.ServerList;
using LunaCommon.Enums;
using UnityEngine;

namespace LunaClient.Windows.Connection
{
    public partial class ConnectionWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.ConnectionWindowText.PlayerName, LabelOptions);
            var oldPlayerName = SettingsSystem.CurrentSettings.PlayerName;
            SettingsSystem.CurrentSettings.PlayerName = GUILayout.TextArea(SettingsSystem.CurrentSettings.PlayerName, 32, TextAreaStyle); // Max 32 characters
            if (oldPlayerName != SettingsSystem.CurrentSettings.PlayerName)
            {
                SettingsSystem.CurrentSettings.PlayerName = SettingsSystem.CurrentSettings.PlayerName.Replace("\n", "");
                RenameEventHandled = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            //Draw add button
            var addMode = SelectedSafe == -1 ? LocalizationContainer.ConnectionWindowText.Add : LocalizationContainer.ConnectionWindowText.Edit;
            var buttonAddMode = addMode;
            if (AddingServer)
                buttonAddMode = LocalizationContainer.ConnectionWindowText.Cancel;
            AddingServer = GUILayout.Toggle(AddingServer, buttonAddMode, ButtonStyle);
            if (AddingServer && !AddingServerSafe && Selected != -1)
            {
                //Load the existing server settings
                ServerName = SettingsSystem.CurrentSettings.Servers[Selected].Name;
                ServerAddress = SettingsSystem.CurrentSettings.Servers[Selected].Address;
                ServerPort = SettingsSystem.CurrentSettings.Servers[Selected].Port.ToString();
            }

            //Draw connect button
            if (MainSystem.NetworkState == ClientState.Disconnected)
            {
                GUI.enabled = SelectedSafe != -1;
                if (GUILayout.Button(LocalizationContainer.ConnectionWindowText.Connect, ButtonStyle))
                    ConnectEventHandled = false;
            }
            else if (MainSystem.NetworkState > ClientState.Disconnected)
            {
                if (GUILayout.Button(LocalizationContainer.ConnectionWindowText.Disconnect, ButtonStyle))
                    DisconnectEventHandled = false;
            }
            //Draw remove button
            if (GUILayout.Button(LocalizationContainer.ConnectionWindowText.Remove, ButtonStyle))
                if (RemoveEventHandled)
                    RemoveEventHandled = false;
            GUI.enabled = true;
            WindowsContainer.Get<OptionsWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<OptionsWindow>().Display, LocalizationContainer.ConnectionWindowText.Options, ButtonStyle);
            if (GUILayout.Button(LocalizationContainer.ConnectionWindowText.Servers, ButtonStyle))
                WindowsContainer.Get<ServerListWindow>().Display = true;
            if(SettingsSystem.CurrentSettings.CloseBtnInConnectionWindow && GUILayout.Button(LocalizationContainer.ConnectionWindowText.Close, ButtonStyle))
                Closed = true;
            GUILayout.EndHorizontal();
            if (AddingServerSafe)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(LocalizationContainer.ConnectionWindowText.Name, LabelOptions);
                ServerName = GUILayout.TextArea(ServerName, TextAreaStyle);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(LocalizationContainer.ConnectionWindowText.Address, LabelOptions);
                ServerAddress = GUILayout.TextArea(ServerAddress, TextAreaStyle).Trim();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(LocalizationContainer.ConnectionWindowText.Port, LabelOptions);
                ServerPort = GUILayout.TextArea(ServerPort, TextAreaStyle).Trim();
                GUILayout.EndHorizontal();
                if (GUILayout.Button($"{addMode} {LocalizationContainer.ConnectionWindowText.Server}", ButtonStyle))
                    if (AddEventHandled)
                        if (Selected == -1)
                        {
                            AddEntry = new ServerEntry
                            {
                                Name = ServerName,
                                Address = ServerAddress,
                                Port = 8800
                            };
                            int.TryParse(ServerPort, out AddEntry.Port);
                            AddEventHandled = false;
                        }
                        else
                        {
                            EditEntry = new ServerEntry
                            {
                                Name = ServerName,
                                Address = ServerAddress,
                                Port = 8800
                            };
                            int.TryParse(ServerPort, out EditEntry.Port);
                            EditEventHandled = false;
                        }
            }

            GUILayout.Label(LocalizationContainer.ConnectionWindowText.CustomServers);
            if (SettingsSystem.CurrentSettings.Servers.Count == 0)
                GUILayout.Label(LocalizationContainer.ConnectionWindowText.NoServers);

            ScrollPos = GUILayout.BeginScrollView(ScrollPos, GUILayout.Width(WindowWidth - 5),
                GUILayout.Height(WindowHeight - 100));

            for (var serverPos = 0; serverPos < SettingsSystem.CurrentSettings.Servers.Count; serverPos++)
            {
                var thisSelected = GUILayout.Toggle(serverPos == SelectedSafe,
                    SettingsSystem.CurrentSettings.Servers[serverPos].Name, ButtonStyle);
                if (Selected == SelectedSafe)
                    if (thisSelected)
                    {
                        if (Selected != serverPos)
                        {
                            Selected = serverPos;
                            AddingServer = false;
                        }
                    }
                    else if (Selected == serverPos)
                    {
                        Selected = -1;
                        AddingServer = false;
                    }
            }
            GUILayout.EndScrollView();

            //Draw Status Message
            GUILayout.Label(Status, StatusStyle);
            GUILayout.EndVertical();
        }
    }
}
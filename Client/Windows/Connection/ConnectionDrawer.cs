using LunaClient.Base.Interface;
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
            GUILayout.Label("Player Name:", LabelOptions);
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
            var addMode = SelectedSafe == -1 ? "Add" : "Edit";
            var buttonAddMode = addMode;
            if (AddingServer)
                buttonAddMode = "Cancel";
            AddingServer = GUILayout.Toggle(AddingServer, buttonAddMode, ButtonStyle);
            if (AddingServer && !AddingServerSafe && (Selected != -1))
            {
                //Load the existing server settings
                ServerName = SettingsSystem.CurrentSettings.Servers[Selected].Name;
                ServerAddress = SettingsSystem.CurrentSettings.Servers[Selected].Address;
                ServerPort = SettingsSystem.CurrentSettings.Servers[Selected].Port.ToString();
            }

            //Draw connect button
            if (MainSystem.Singleton.NetworkState == ClientState.DISCONNECTED)
            {
                GUI.enabled = SelectedSafe != -1;
                if (GUILayout.Button("Connect", ButtonStyle))
                    ConnectEventHandled = false;
            }
            else
            {
                if (GUILayout.Button("Disconnect", ButtonStyle))
                    DisconnectEventHandled = false;
            }
            //Draw remove button
            if (GUILayout.Button("Remove", ButtonStyle))
                if (RemoveEventHandled)
                    RemoveEventHandled = false;
            GUI.enabled = true;
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, "Options", ButtonStyle);
            if (GUILayout.Button("Servers", ButtonStyle))
                ServerListWindow.Singleton.Display = true;
            GUILayout.EndHorizontal();
            if (AddingServerSafe)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name:", LabelOptions);
                ServerName = GUILayout.TextArea(ServerName, TextAreaStyle);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Address:", LabelOptions);
                ServerAddress = GUILayout.TextArea(ServerAddress, TextAreaStyle).Trim();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Port:", LabelOptions);
                ServerPort = GUILayout.TextArea(ServerPort, TextAreaStyle).Trim();
                GUILayout.EndHorizontal();
                if (GUILayout.Button(addMode + " server", ButtonStyle))
                    if (AddEventHandled)
                        if (Selected == -1)
                        {
                            AddEntry = new ServerEntry
                            {
                                Name = ServerName,
                                Address = ServerAddress,
                                Port = 6702
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
                                Port = 6702
                            };
                            int.TryParse(ServerPort, out EditEntry.Port);
                            EditEventHandled = false;
                        }
            }

            GUILayout.Label("Custom servers:");
            if (SettingsSystem.CurrentSettings.Servers.Count == 0)
                GUILayout.Label("(None - Add a server first)");

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
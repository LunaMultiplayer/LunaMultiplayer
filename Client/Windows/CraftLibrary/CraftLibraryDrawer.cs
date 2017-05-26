using LunaClient.Systems.SettingsSys;
using UnityEngine;

namespace LunaClient.Windows.CraftLibrary
{
    public partial class CraftLibraryWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            //Draw the player buttons
            PlayerScrollPos = GUILayout.BeginScrollView(PlayerScrollPos, ScrollStyle);
            DrawPlayerButton(SettingsSystem.CurrentSettings.PlayerName);
            foreach (var playerName in System.PlayersWithCrafts)
                if (playerName != SettingsSystem.CurrentSettings.PlayerName)
                    DrawPlayerButton(playerName);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawPlayerButton(string playerName)
        {
            var buttonSelected = GUILayout.Toggle(System.SelectedPlayer == playerName, playerName, ButtonStyle);
            if (buttonSelected && (System.SelectedPlayer != playerName))
                System.SelectedPlayer = playerName;
            if (!buttonSelected && (System.SelectedPlayer == playerName))
                System.SelectedPlayer = null;
        }

        public void DrawLibraryContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            var newShowUpload = false;
            if (System.SelectedPlayer == SettingsSystem.CurrentSettings.PlayerName)
                newShowUpload = GUILayout.Toggle(ShowUpload, "Upload", ButtonStyle);
            if (newShowUpload && !ShowUpload)
                System.BuildUploadList();
            ShowUpload = newShowUpload;
            LibraryScrollPos = GUILayout.BeginScrollView(LibraryScrollPos, ScrollStyle);
            if (ShowUpload)
                DrawUploadScreen();
            else
                DrawDownloadScreen();
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawUploadScreen()
        {
            foreach (var entryType in System.UploadList)
            {
                GUILayout.Label(entryType.Key.ToString(), LabelStyle);
                foreach (var entryName in entryType.Value)
                {
                    if (System.PlayerList.ContainsKey(SettingsSystem.CurrentSettings.PlayerName) &&
                        System.PlayerList[SettingsSystem.CurrentSettings.PlayerName].ContainsKey(entryType.Key) &&
                        System.PlayerList[SettingsSystem.CurrentSettings.PlayerName][entryType.Key].Contains(entryName))
                        GUI.enabled = false;
                    if (GUILayout.Button(entryName, ButtonStyle))
                    {
                        System.UploadCraftType = entryType.Key;
                        System.UploadCraftName = entryName;
                    }
                    GUI.enabled = true;
                }
            }
        }

        private void DrawDownloadScreen()
        {
            if (System.PlayerList.ContainsKey(System.SelectedPlayer))
                foreach (var entry in System.PlayerList[System.SelectedPlayer])
                {
                    GUILayout.Label(entry.Key.ToString(), LabelStyle);
                    foreach (var craftName in entry.Value)
                        if (System.SelectedPlayer == SettingsSystem.CurrentSettings.PlayerName)
                        {
                            //Also draw remove button on player screen
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button(craftName, ButtonStyle))
                            {
                                System.DownloadCraftType = entry.Key;
                                System.DownloadCraftName = craftName;
                            }
                            if (GUILayout.Button("Remove", ButtonStyle))
                            {
                                System.DeleteCraftType = entry.Key;
                                System.DeleteCraftName = craftName;
                            }
                            GUILayout.EndHorizontal();
                        }
                        else
                        {
                            if (GUILayout.Button(craftName, ButtonStyle))
                            {
                                System.DownloadCraftType = entry.Key;
                                System.DownloadCraftName = craftName;
                            }
                        }
                }
        }
    }
}
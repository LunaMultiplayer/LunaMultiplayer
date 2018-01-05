using LunaClient.Systems;
using LunaClient.Systems.Chat;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaClient.Windows.Chat;
using LunaClient.Windows.CraftLibrary;
using LunaClient.Windows.Debug;
using LunaClient.Windows.Options;
using LunaClient.Windows.Systems;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Windows.Status
{
    public partial class StatusWindow
    {
        private static readonly WarpSystem WarpSystem = SystemsContainer.Get<WarpSystem>();

        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            #region Horizontal toolbar

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            var chatButtonStyle = ButtonStyle;
            if (SystemsContainer.Get<ChatSystem>().ChatButtonHighlighted)
                chatButtonStyle = HighlightStyle;
            if (!SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                var tooltip = "Drops control of the vessels that you are not controlling so other players can control them";
                if (GUILayout.Button(new GUIContent("DropCtrl", tooltip), ButtonStyle))
                {
                    SystemsContainer.Get<VesselLockSystem>().DropAllOtherVesselControlLocks();
                }
            }
            WindowsContainer.Get<ChatWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<ChatWindow>().Display, "Chat", chatButtonStyle);
            WindowsContainer.Get<CraftLibraryWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<CraftLibraryWindow>().Display, "Craft", ButtonStyle);
            WindowsContainer.Get<DebugWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<DebugWindow>().Display, "Debug", ButtonStyle);
#if DEBUG
            WindowsContainer.Get<SystemsWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<SystemsWindow>().Display, "Systems", ButtonStyle);
#endif

            GUILayout.EndHorizontal();

            #endregion

            #region Players information

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, ScrollStyle);

            //Draw other subspaces
            foreach (var currentEntry in SubspaceDisplay)
            {
                if (currentEntry.SubspaceId == -1)
                {
                    //Draw the warping players
                    GUILayout.BeginHorizontal(SubspaceStyle);
                    GUILayout.Label("WARPING");
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal(SubspaceStyle);
                    GUILayout.Label($"T: +{KSPUtil.PrintTimeCompact(WarpSystem.GetSubspaceTime(currentEntry.SubspaceId), false)}");
                    GUILayout.FlexibleSpace();
                    if (NotWarpingAndIsFutureSubspace(currentEntry.SubspaceId) && GUILayout.Button("Sync", ButtonStyle))
                        WarpSystem.CurrentSubspace = currentEntry.SubspaceId;
                    GUILayout.EndHorizontal();
                }

                foreach (var currentPlayer in currentEntry.Players)
                {
                    DrawPlayerEntry(currentPlayer == SettingsSystem.CurrentSettings.PlayerName
                        ? SystemsContainer.Get<StatusSystem>().MyPlayerStatus
                        : SystemsContainer.Get<StatusSystem>().GetPlayerStatus(currentPlayer));
                }
            }

            GUILayout.EndScrollView();

            #endregion

            GUILayout.FlexibleSpace();
#if DEBUG
            GUILayout.BeginHorizontal();
            DrawDebugSwitches();
            GUILayout.EndHorizontal();
#endif
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disconnect", ButtonStyle))
                DisconnectEventHandled = false;
            WindowsContainer.Get<OptionsWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<OptionsWindow>().Display, "Options", ButtonStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawDebugSwitches()
        {
#if DEBUG
            var d1 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug1, "D1", ButtonStyle);
            if (d1 != SettingsSystem.CurrentSettings.Debug1)
            {
                CommonUtil.Reserve20Mb();
                CommonUtil.Reserve20Mb();
                CommonUtil.Reserve20Mb();
                CommonUtil.Reserve20Mb();

                SettingsSystem.CurrentSettings.Debug1 = d1;
                SettingsSystem.SaveSettings();
            }
            var d2 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug2, "D2", ButtonStyle);
            if (d2 != SettingsSystem.CurrentSettings.Debug2)
            {
                SettingsSystem.CurrentSettings.Debug2 = d2;
                SettingsSystem.SaveSettings();
            }
            var d3 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug3, "D3", ButtonStyle);
            if (d3 != SettingsSystem.CurrentSettings.Debug3)
            {
                SettingsSystem.CurrentSettings.Debug3 = d3;
                SettingsSystem.SaveSettings();
            }
            var d4 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug4, "D4", ButtonStyle);
            if (d4 != SettingsSystem.CurrentSettings.Debug4)
            {
                SettingsSystem.CurrentSettings.Debug4 = d4;
                SettingsSystem.SaveSettings();
            }
            var d5 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug5, "D5", ButtonStyle);
            if (d5 != SettingsSystem.CurrentSettings.Debug5)
            {
                SettingsSystem.CurrentSettings.Debug5 = d5;
                SettingsSystem.SaveSettings();
            }
            var d6 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug6, "D6", ButtonStyle);
            if (d6 != SettingsSystem.CurrentSettings.Debug6)
            {
                SettingsSystem.CurrentSettings.Debug6 = d6;
                SettingsSystem.SaveSettings();
            }
            var d7 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug7, "D7", ButtonStyle);
            if (d7 != SettingsSystem.CurrentSettings.Debug7)
            {
                SettingsSystem.CurrentSettings.Debug7 = d7;
                SettingsSystem.SaveSettings();
            }
            var d8 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug8, "D8", ButtonStyle);
            if (d8 != SettingsSystem.CurrentSettings.Debug8)
            {
                SettingsSystem.CurrentSettings.Debug8 = d8;
                SettingsSystem.SaveSettings();
            }
            var d9 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug9, "D9", ButtonStyle);
            if (d9 != SettingsSystem.CurrentSettings.Debug9)
            {
                SettingsSystem.CurrentSettings.Debug9 = d9;
                SettingsSystem.SaveSettings();
            }
#endif
        }

        private static bool NotWarpingAndIsFutureSubspace(int subspaceId)
        {
            return !WarpSystem.CurrentlyWarping && WarpSystem.CurrentSubspace != subspaceId &&
                   WarpSystem.Subspaces.ContainsKey(WarpSystem.CurrentSubspace) && WarpSystem.Subspaces.ContainsKey(subspaceId) &&
                   WarpSystem.Subspaces[WarpSystem.CurrentSubspace] < WarpSystem.Subspaces[subspaceId];
        }
        
        private void DrawPlayerEntry(PlayerStatus playerStatus)
        {
            if (playerStatus == null)
                return;
            GUILayout.BeginHorizontal();
            if (!PlayerNameStyle.ContainsKey(playerStatus.PlayerName))
            {
                PlayerNameStyle[playerStatus.PlayerName] = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = SystemsContainer.Get<PlayerColorSystem>().GetPlayerColor(playerStatus.PlayerName) },
                    hover = { textColor = SystemsContainer.Get<PlayerColorSystem>().GetPlayerColor(playerStatus.PlayerName) },
                    active = { textColor = SystemsContainer.Get<PlayerColorSystem>().GetPlayerColor(playerStatus.PlayerName) },
                    fontStyle = FontStyle.Bold,
                    stretchWidth = true,
                    wordWrap = false
                };
            }
            GUILayout.Label(playerStatus.PlayerName, PlayerNameStyle[playerStatus.PlayerName]);
            GUILayout.FlexibleSpace();
            GUILayout.Label(playerStatus.StatusText, StateTextStyle);
            GUILayout.Label(playerStatus.VesselText != "" ? $"Pilot: {playerStatus.VesselText}" : "", VesselNameStyle);
            GUILayout.EndHorizontal();
        }
    }
}
using LunaClient.Systems.Chat;
using LunaClient.Systems.ColorSystem;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.Warp;
using LunaClient.Windows.Chat;
using LunaClient.Windows.CraftLibrary;
using LunaClient.Windows.Debug;
using LunaClient.Windows.Options;
using LunaClient.Windows.Systems;
using LunaCommon;
using UniLinq;
using UnityEngine;

namespace LunaClient.Windows.Status
{
    public partial class StatusWindow
    {
        public void DrawContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            #region Horizontal toolbar

            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            var chatButtonStyle = ButtonStyle;
            if (ChatSystem.Singleton.ChatButtonHighlighted)
                chatButtonStyle = HighlightStyle;
            if (!SettingsSystem.ServerSettings.DropControlOnVesselSwitching)
            {
                var tooltip = "Drops control of the vessels that you are not controlling so other players can control them";
                if (GUILayout.Button(new GUIContent("DropCtrl", tooltip), ButtonStyle))
                {
                    VesselLockSystem.Singleton.DropAllOtherVesselControlLocks();
                }
            }
            ChatWindow.Singleton.Display = GUILayout.Toggle(ChatWindow.Singleton.Display, "Chat", chatButtonStyle);
            CraftLibraryWindow.Singleton.Display = GUILayout.Toggle(CraftLibraryWindow.Singleton.Display, "Craft", ButtonStyle);
            DebugWindow.Singleton.Display = GUILayout.Toggle(DebugWindow.Singleton.Display, "Debug", ButtonStyle);
#if DEBUG
            SystemsWindow.Singleton.Display = GUILayout.Toggle(SystemsWindow.Singleton.Display, "Systems", ButtonStyle);
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
                    GUILayout.Label("T: +" + KSPUtil.PrintTimeCompact(WarpSystem.Singleton.GetSubspaceTime(currentEntry.SubspaceId), false));
                    GUILayout.FlexibleSpace();
                    if (NotWarpingAndIsFutureSubspace(currentEntry.SubspaceId) && GUILayout.Button("Sync", ButtonStyle))
                        WarpSystem.Singleton.CurrentSubspace = currentEntry.SubspaceId;
                    GUILayout.EndHorizontal();
                }
                
                foreach (var currentPlayer in currentEntry.Players)
                {
                    DrawPlayerEntry(currentPlayer == SettingsSystem.CurrentSettings.PlayerName
                        ? StatusSystem.Singleton.MyPlayerStatus
                        : StatusSystem.Singleton.GetPlayerStatus(currentPlayer));
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
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, "Options", ButtonStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawDebugSwitches()
        {
#if DEBUG
            var d1 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug1, "D1", ButtonStyle);
            if (d1 != SettingsSystem.CurrentSettings.Debug1)
            {
                SettingsSystem.CurrentSettings.Debug1 = d1;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d2 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug2, "D2", ButtonStyle);
            if (d2 != SettingsSystem.CurrentSettings.Debug2)
            {
                SettingsSystem.CurrentSettings.Debug2 = d2;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d3 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug3, "D3", ButtonStyle);
            if (d3 != SettingsSystem.CurrentSettings.Debug3)
            {
                SettingsSystem.CurrentSettings.Debug3 = d3;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d4 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug4, "D4", ButtonStyle);
            if (d4 != SettingsSystem.CurrentSettings.Debug4)
            {
                SettingsSystem.CurrentSettings.Debug4 = d4;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d5 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug5, "D5", ButtonStyle);
            if (d5 != SettingsSystem.CurrentSettings.Debug5)
            {
                SettingsSystem.CurrentSettings.Debug5 = d5;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d6 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug6, "D6", ButtonStyle);
            if (d6 != SettingsSystem.CurrentSettings.Debug6)
            {
                SettingsSystem.CurrentSettings.Debug6 = d6;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d7 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug7, "D7", ButtonStyle);
            if (d7 != SettingsSystem.CurrentSettings.Debug7)
            {
                SettingsSystem.CurrentSettings.Debug7 = d7;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d8 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug8, "D8", ButtonStyle);
            if (d8 != SettingsSystem.CurrentSettings.Debug8)
            {
                SettingsSystem.CurrentSettings.Debug8 = d8;
                SettingsSystem.Singleton.SaveSettings();
            }
            var d9 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug9, "D9", ButtonStyle);
            if (d9 != SettingsSystem.CurrentSettings.Debug9)
            {
                SettingsSystem.CurrentSettings.Debug9 = d9;
                SettingsSystem.Singleton.SaveSettings();
            }
#endif
        }

        private static bool NotWarpingAndIsFutureSubspace(int subspaceId)
        {
            return !WarpSystem.Singleton.CurrentlyWarping && WarpSystem.Singleton.CurrentSubspace != subspaceId &&
                WarpSystem.Singleton.Subspaces.ContainsKey(WarpSystem.Singleton.CurrentSubspace) &&
                WarpSystem.Singleton.Subspaces[WarpSystem.Singleton.CurrentSubspace] < WarpSystem.Singleton.Subspaces[subspaceId];
        }

        public void DrawMaximize(int windowId)
        {
            GUI.DragWindow(MoveRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            var chatButtonStyle = ButtonStyle;
            if (ChatSystem.Singleton.ChatButtonHighlighted)
                chatButtonStyle = HighlightStyle;
            ChatWindow.Singleton.Display = GUILayout.Toggle(ChatWindow.Singleton.Display, "C", chatButtonStyle);
            DebugWindow.Singleton.Display = GUILayout.Toggle(DebugWindow.Singleton.Display, "D", ButtonStyle);
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, "O", ButtonStyle);
#if DEBUG
            SystemsWindow.Singleton.Display = GUILayout.Toggle(SystemsWindow.Singleton.Display, "S", ButtonStyle);
#endif
            if (GUILayout.Button("+", ButtonStyle))
            {
                WindowRect.xMax = MinWindowRect.xMax;
                WindowRect.yMin = MinWindowRect.yMin;
                WindowRect.xMin = MinWindowRect.xMax - WindowWidth;
                WindowRect.yMax = MinWindowRect.yMin + WindowHeight;
                Minmized = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawPlayerEntry(PlayerStatus playerStatus)
        {
            if (playerStatus == null)
                return;
            GUILayout.BeginHorizontal();
            if (!PlayerNameStyle.ContainsKey(playerStatus.PlayerName))
            {
                PlayerNameStyle[playerStatus.PlayerName] = new GUIStyle(GUI.skin.label);
                PlayerNameStyle[playerStatus.PlayerName].normal.textColor =
                    PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName);
                PlayerNameStyle[playerStatus.PlayerName].hover.textColor =
                    PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName);
                PlayerNameStyle[playerStatus.PlayerName].active.textColor =
                    PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName);
                PlayerNameStyle[playerStatus.PlayerName].fontStyle = FontStyle.Bold;
                PlayerNameStyle[playerStatus.PlayerName].stretchWidth = true;
                PlayerNameStyle[playerStatus.PlayerName].wordWrap = false;
            }
            GUILayout.Label(playerStatus.PlayerName, PlayerNameStyle[playerStatus.PlayerName]);
            GUILayout.FlexibleSpace();
            GUILayout.Label(playerStatus.StatusText, StateTextStyle);
            GUILayout.EndHorizontal();
            if (playerStatus.VesselText != "")
                GUILayout.Label("Pilot: " + playerStatus.VesselText, VesselNameStyle);
        }
    }
}
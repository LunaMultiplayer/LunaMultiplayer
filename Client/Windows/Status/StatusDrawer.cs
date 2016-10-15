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
using LunaCommon;
using LunaCommon.Enums;
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

            GUILayout.EndHorizontal();

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, ScrollStyle);

            //Draw the server subspace and the players in it
            if (SettingsSystem.ServerSettings.WarpMode == WarpMode.SUBSPACE)
            {
                GUILayout.BeginHorizontal(SubspaceStyle);
                GUILayout.Label("Server T: " + KSPUtil.PrintTimeCompact(WarpSystem.Singleton.GetSubspaceTime(0), false));
                if (NotWarpingAndNotInGivenSubspace(0) && GUILayout.Button("Sync", ButtonStyle))
                    WarpSystem.Singleton.CurrentSubspace = 0;
                GUILayout.EndHorizontal();

                var playersInServerSubspace = SubspaceDisplay.FirstOrDefault(s => s.SubspaceId == 0);
                if (playersInServerSubspace != null)
                {
                    foreach (var currentPlayer in playersInServerSubspace.Players)
                    {
                        DrawPlayerEntry(currentPlayer == SettingsSystem.CurrentSettings.PlayerName
                            ? StatusSystem.Singleton.MyPlayerStatus
                            : StatusSystem.Singleton.GetPlayerStatus(currentPlayer));
                    }
                }
            }

            //Draw other subspaces
            foreach (var currentEntry in SubspaceDisplay.Where(s=> s.SubspaceId != 0))
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
                    if (NotWarpingAndNotInGivenSubspace(currentEntry.SubspaceId) && GUILayout.Button("Sync", ButtonStyle))
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
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Disconnect", ButtonStyle))
                DisconnectEventHandled = false;
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, "Options", ButtonStyle);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private static bool NotWarpingAndNotInGivenSubspace(int subspaceId)
        {
            return !WarpSystem.Singleton.CurrentlyWarping && WarpSystem.Singleton.CurrentSubspace != subspaceId;
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
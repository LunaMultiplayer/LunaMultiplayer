using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Status;
using LunaClient.Systems.Warp;
using LunaClient.Windows.Chat;
using LunaClient.Windows.CraftLibrary;
using LunaClient.Windows.Debug;
using LunaClient.Windows.Locks;
using LunaClient.Windows.Options;
using LunaClient.Windows.Screenshots;
using LunaClient.Windows.Systems;
using LunaCommon;
using UnityEngine;

namespace LunaClient.Windows.Status
{
    public partial class StatusWindow
    {
        private static readonly WarpSystem WarpSystem = WarpSystem.Singleton;

        public override void DrawWindowContent(int windowId)
        {
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);

            #region Horizontal toolbar

            GUILayout.BeginHorizontal();

            ChatWindow.Singleton.Display = GUILayout.Toggle(ChatWindow.Singleton.Display, ChatIcon, ButtonStyle);
            CraftLibraryWindow.Singleton.Display = GUILayout.Toggle(CraftLibraryWindow.Singleton.Display, RocketIcon, ButtonStyle);
            ScreenshotsWindow.Singleton.Display = GUILayout.Toggle(ScreenshotsWindow.Singleton.Display, CameraIcon, ButtonStyle);
#if DEBUG
            DebugWindow.Singleton.Display = GUILayout.Toggle(DebugWindow.Singleton.Display, StatusTexts.DebugBtnTxt, ButtonStyle);
            SystemsWindow.Singleton.Display = GUILayout.Toggle(SystemsWindow.Singleton.Display, StatusTexts.SystemsBtnTxt, ButtonStyle);
            LocksWindow.Singleton.Display = GUILayout.Toggle(LocksWindow.Singleton.Display, StatusTexts.LocksBtnTxt, ButtonStyle);
#endif

            GUILayout.EndHorizontal();

            #endregion

            #region Players information

            ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, ScrollStyle);
            for (var i = 0; i < SubspaceDisplay.Count; i++)
            {
                GUILayout.BeginHorizontal(SubspaceStyle);
                if (SubspaceDisplay[i].SubspaceId == -1)
                {
                    GUILayout.Label(StatusTexts.WarpingLabelTxt);
                }
                else
                {
                    GUILayout.Label(StatusTexts.GetTimeLabel(SubspaceDisplay[i]));
                    GUILayout.FlexibleSpace();
                    if (NotWarpingAndIsFutureSubspace(SubspaceDisplay[i].SubspaceId) && GUILayout.Button(StatusTexts.SyncBtnTxt, ButtonStyle))
                        WarpSystem.CurrentSubspace = SubspaceDisplay[i].SubspaceId;
                }
                GUILayout.EndHorizontal();
                for (var j = 0; j < SubspaceDisplay[i].Players.Count; j++)
                {
                    DrawPlayerEntry(SubspaceDisplay[i].Players[j] == SettingsSystem.CurrentSettings.PlayerName
                        ? StatusSystem.Singleton.MyPlayerStatus
                        : StatusSystem.Singleton.GetPlayerStatus(SubspaceDisplay[i].Players[j]));
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
            if (GUILayout.Button(StatusTexts.DisconnectBtnTxt, ButtonStyle))
                DisconnectEventHandled = false;
            OptionsWindow.Singleton.Display = GUILayout.Toggle(OptionsWindow.Singleton.Display, StatusTexts.OptionsBtnTxt, ButtonStyle);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawDebugSwitches()
        {
#if DEBUG
            var d1 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug1, StatusTexts.Debug1BtnTxt, ButtonStyle);
            if (d1 != SettingsSystem.CurrentSettings.Debug1)
            {
                SettingsSystem.CurrentSettings.Debug1 = d1;
                SettingsSystem.SaveSettings();
            }
            var d2 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug2, StatusTexts.Debug2BtnTxt, ButtonStyle);
            if (d2 != SettingsSystem.CurrentSettings.Debug2)
            {
                SettingsSystem.CurrentSettings.Debug2 = d2;
                SettingsSystem.SaveSettings();
            }
            var d3 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug3, StatusTexts.Debug3BtnTxt, ButtonStyle);
            if (d3 != SettingsSystem.CurrentSettings.Debug3)
            {
                SettingsSystem.CurrentSettings.Debug3 = d3;
                SettingsSystem.SaveSettings();
            }
            var d4 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug4, StatusTexts.Debug4BtnTxt, ButtonStyle);
            if (d4 != SettingsSystem.CurrentSettings.Debug4)
            {
                SettingsSystem.CurrentSettings.Debug4 = d4;
                SettingsSystem.SaveSettings();
            }
            var d5 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug5, StatusTexts.Debug5BtnTxt, ButtonStyle);
            if (d5 != SettingsSystem.CurrentSettings.Debug5)
            {
                SettingsSystem.CurrentSettings.Debug5 = d5;
                SettingsSystem.SaveSettings();
            }
            var d6 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug6, StatusTexts.Debug6BtnTxt, ButtonStyle);
            if (d6 != SettingsSystem.CurrentSettings.Debug6)
            {
                SettingsSystem.CurrentSettings.Debug6 = d6;
                SettingsSystem.SaveSettings();
            }
            var d7 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug7, StatusTexts.Debug7BtnTxt, ButtonStyle);
            if (d7 != SettingsSystem.CurrentSettings.Debug7)
            {
                SettingsSystem.CurrentSettings.Debug7 = d7;
                SettingsSystem.SaveSettings();
            }
            var d8 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug8, StatusTexts.Debug8BtnTxt, ButtonStyle);
            if (d8 != SettingsSystem.CurrentSettings.Debug8)
            {
                SettingsSystem.CurrentSettings.Debug8 = d8;
                SettingsSystem.SaveSettings();
            }
            var d9 = GUILayout.Toggle(SettingsSystem.CurrentSettings.Debug9, StatusTexts.Debug9BtnTxt, ButtonStyle);
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
                    normal = { textColor = PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName) },
                    hover = { textColor = PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName) },
                    active = { textColor = PlayerColorSystem.Singleton.GetPlayerColor(playerStatus.PlayerName) },
                    fontStyle = FontStyle.Bold,
                    stretchWidth = true,
                    wordWrap = false
                };
            }
            GUILayout.Label(playerStatus.PlayerName, PlayerNameStyle[playerStatus.PlayerName]);
            GUILayout.FlexibleSpace();
            GUILayout.Label(playerStatus.StatusText, StateTextStyle);
            GUILayout.Label(string.IsNullOrEmpty(playerStatus.VesselText) ? string.Empty : StatusTexts.GetPlayerText(playerStatus), VesselNameStyle);
            GUILayout.EndHorizontal();
        }
    }
}
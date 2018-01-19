using LunaClient.Systems;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Toolbar;
using LunaClient.Windows.Status;
using LunaClient.Windows.UniverseConverter;
using LunaCommon.Enums;
using System;
using UnityEngine;

namespace LunaClient.Windows.Options
{
    public partial class OptionsWindow
    {
        public void DrawContent(int windowId)
        {
            if (!LoadEventHandled)
            {
                LoadEventHandled = true;
                TempColor = SettingsSystem.CurrentSettings.PlayerColor;
            }
            //Player color
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name color: ");
            GUILayout.Label(SettingsSystem.CurrentSettings.PlayerName, TempColorLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("R: ");
            TempColor.r = GUILayout.HorizontalScrollbar(TempColor.r, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("G: ");
            TempColor.g = GUILayout.HorizontalScrollbar(TempColor.g, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("B: ");
            TempColor.b = GUILayout.HorizontalScrollbar(TempColor.b, 0, 0, 1);
            GUILayout.EndHorizontal();
            TempColorLabelStyle.active.textColor = TempColor;
            TempColorLabelStyle.normal.textColor = TempColor;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Random", ButtonStyle))
                TempColor = PlayerColorSystem.GenerateRandomColor();
            if (GUILayout.Button("Set", ButtonStyle))
            {
                WindowsContainer.Get<StatusWindow>().ColorEventHandled = false;
                SettingsSystem.CurrentSettings.PlayerColor = TempColor;
                SettingsSystem.SaveSettings();
                if (MainSystem.NetworkState == ClientState.Running)
                    SystemsContainer.Get<PlayerColorSystem>().MessageSender.SendPlayerColorToServer();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            //Key bindings
            var chatDescription = $"Set chat key (current: {SettingsSystem.CurrentSettings.ChatKey})";
            if (SettingChat)
            {
                chatDescription = "Setting chat key (click to cancel)...";
                if (Event.current.isKey)
                    if (Event.current.keyCode != KeyCode.Escape)
                    {
                        SettingsSystem.CurrentSettings.ChatKey = Event.current.keyCode;
                        SettingsSystem.SaveSettings();
                        SettingChat = false;
                    }
                    else
                    {
                        SettingChat = false;
                    }
            }
            if (GUILayout.Button(chatDescription))
                SettingChat = !SettingChat;
            if (GUILayout.Button("Reset disclaimer"))
            {
                SettingsSystem.CurrentSettings.DisclaimerAccepted = false;
                SettingsSystem.SaveSettings();
            }
            var settingCompression = GUILayout.Toggle(SettingsSystem.CurrentSettings.CompressionEnabled, "Enable compression", ButtonStyle);
            if (settingCompression != SettingsSystem.CurrentSettings.CompressionEnabled)
            {
                SettingsSystem.CurrentSettings.CompressionEnabled = settingCompression;
                SettingsSystem.SaveSettings();
            }
            var settingInterpolation = GUILayout.Toggle(SettingsSystem.CurrentSettings.InterpolationEnabled, "Enable interpolation", ButtonStyle);
            if (settingInterpolation != SettingsSystem.CurrentSettings.InterpolationEnabled)
            {
                SettingsSystem.CurrentSettings.InterpolationEnabled = settingInterpolation;
                SettingsSystem.SaveSettings();
            }
            var closeBtnInConnectionWindow = GUILayout.Toggle(SettingsSystem.CurrentSettings.CloseBtnInConnectionWindow, "Show \"Close\" button in connection window", ButtonStyle);
            if (closeBtnInConnectionWindow != SettingsSystem.CurrentSettings.CloseBtnInConnectionWindow)
            {
                SettingsSystem.CurrentSettings.CloseBtnInConnectionWindow = closeBtnInConnectionWindow;
                SettingsSystem.SaveSettings();
            }
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUILayout.Label("Positioning system:");
            var positionSetting = GUILayout.SelectionGrid(SettingsSystem.CurrentSettings.PositionSystem, new[] { "Dark", "Dagger", "Mixed" }, 3, "toggle");
            if (positionSetting != SettingsSystem.CurrentSettings.PositionSystem)
            {
                SettingsSystem.CurrentSettings.PositionSystem = positionSetting;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.Label("Generate a server LMPModControl:");
            if (GUILayout.Button("Generate blacklist LMPModControl.txt"))
                SystemsContainer.Get<ModSystem>().GenerateModControlFile(false);
            if (GUILayout.Button("Generate whitelist LMPModControl.txt"))
                SystemsContainer.Get<ModSystem>().GenerateModControlFile(true);
            WindowsContainer.Get<UniverseConverterWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<UniverseConverterWindow>().Display, "Generate Universe from saved game", ButtonStyle);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Toolbar:", SmallOption);
            if (GUILayout.Button(ToolbarMode, ButtonStyle))
            {
                var newSetting = (int)SettingsSystem.CurrentSettings.ToolbarType + 1;
                //Overflow to 0
                if (!Enum.IsDefined(typeof(LmpToolbarType), newSetting))
                    newSetting = 0;
                SettingsSystem.CurrentSettings.ToolbarType = (LmpToolbarType)newSetting;
                SettingsSystem.SaveSettings();
                UpdateToolbarString();
                SystemsContainer.Get<ToolbarSystem>().ToolbarChanged();
            }
            GUILayout.EndHorizontal();
#if DEBUG
            if (GUILayout.Button("Check Common.dll stock parts"))
                SystemsContainer.Get<ModSystem>().CheckCommonStockParts();
#endif
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }
    }
}
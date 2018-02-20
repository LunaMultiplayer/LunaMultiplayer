using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Toolbar;
using LunaClient.Windows.Status;
using LunaClient.Windows.UniverseConverter;
using LunaCommon.Enums;
using LunaCommon.Time;
using System;
using System.Globalization;
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
            GUILayout.Label(LocalizationContainer.SettingsWindowText.Language);
            if (GUILayout.Button(LocalizationContainer.CurrentLanguage.ToString(), ButtonStyle))
            {
                LocalizationContainer.LoadLanguage(LocalizationContainer.GetNextLanguage());
                SettingsSystem.CurrentSettings.Language = LocalizationContainer.CurrentLanguage;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Name color: ");
            GUILayout.Label(SettingsSystem.CurrentSettings.PlayerName, TempColorLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("R: ", SmallOption);
            TempColor.r = GUILayout.HorizontalScrollbar(TempColor.r, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("G: ", SmallOption);
            TempColor.g = GUILayout.HorizontalScrollbar(TempColor.g, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("B: ", SmallOption);
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
            DrawAdvancedDebugOptions();
#endif
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close", ButtonStyle))
                Display = false;
            GUILayout.EndVertical();
        }

        private void DrawAdvancedDebugOptions()
        {
            if (GUILayout.Button("Check Common.dll stock parts"))
                SystemsContainer.Get<ModSystem>().CheckCommonStockParts();
            GUILayout.Space(10);

            ShowAdvancedNetworkFields = GUILayout.Toggle(ShowAdvancedNetworkFields, "Advanced network fields", ButtonStyle);
            if (ShowAdvancedNetworkFields)
            {
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label("Cannot change values while connected");
                }

                GUILayout.Label($"MTU: {NetworkMain.Config.MaximumTransmissionUnit}");
                //if (MainSystem.NetworkState <= ClientState.Disconnected)
                //    NetworkMain.Config.MaximumTransmissionUnit = (int)GUILayout.HorizontalScrollbar(NetworkMain.Config.MaximumTransmissionUnit, 0, 0, 10000);

                GUILayout.Label(InfiniteTimeout
                    ? "Timeout: Infinite"
                    : $"Timeout: {NetworkMain.Config.ConnectionTimeout}s. Minimum: {NetworkMain.Config.PingInterval}s");

                GUILayout.BeginHorizontal();
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    InfiniteTimeout = GUILayout.Toggle(InfiniteTimeout, "Infinite", "toggle");
                    if (!InfiniteTimeout)
                    {
                        if (NetworkMain.Config.ConnectionTimeout >= float.MaxValue - 1)
                            NetworkMain.Config.ConnectionTimeout = 15;
                        
                        var value = GUILayout.TextArea(NetworkMain.Config.ConnectionTimeout.ToString(CultureInfo.InvariantCulture), 32, TextAreaStyle);
                        if (float.TryParse(value, out var parsedResult))
                        {
                            if (parsedResult >= NetworkMain.Config.PingInterval)
                                NetworkMain.Config.ConnectionTimeout = parsedResult;
                        }
                    }
                    else
                    {
                        NetworkMain.Config.ConnectionTimeout = float.MaxValue;
                    }
                }
                GUILayout.EndHorizontal();
            }

            ShowBadNetworkSimulationFields = GUILayout.Toggle(ShowBadNetworkSimulationFields, "Bad network simulation", ButtonStyle);
            if (ShowBadNetworkSimulationFields)
            {
                GUILayout.Label($"NTP time offset: {LunaTime.SimulatedMsTimeOffset:F1}ms");
                LunaTime.SimulatedMsTimeOffset = (float) Math.Round(GUILayout.HorizontalScrollbar(LunaTime.SimulatedMsTimeOffset, 0, -2500, 2500), 3);
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label("Cannot change values while connected");
                }

                GUILayout.Label($"Packet loss: {NetworkMain.Config.SimulatedLoss * 100:F1}%");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedLoss = (float) Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedLoss, 0, 0, 1), 3);
                GUILayout.Label($"Packet duplication: {NetworkMain.Config.SimulatedDuplicatesChance * 100:F1}%");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedDuplicatesChance = (float) Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedDuplicatesChance, 0, 0, 1), 3);
                GUILayout.Label($"Max random latency: {NetworkMain.Config.SimulatedRandomLatency * 1000:F1} ms");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedRandomLatency = (float) Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedRandomLatency, 0, 0, 3), 4);
                GUILayout.Label($"Min latency: {NetworkMain.Config.SimulatedMinimumLatency * 1000:F1} ms");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedMinimumLatency = (float) Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedMinimumLatency, 0, 0, 3),4);
            }
        }
    }
}
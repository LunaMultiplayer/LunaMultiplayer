using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
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
        public override void DrawWindowContent(int windowId)
        {
            if (!LoadEventHandled)
            {
                LoadEventHandled = true;
                TempColor = SettingsSystem.CurrentSettings.PlayerColor;
            }
            //Player color
            GUILayout.BeginVertical(BoxStyle);
            GUI.DragWindow(MoveRect);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Language);
            if (GUILayout.Button(LocalizationContainer.GetCurrentLanguageAsText(), ButtonStyle))
            {
                LocalizationContainer.LoadLanguage(LocalizationContainer.GetNextLanguage());
                SettingsSystem.CurrentSettings.Language = LocalizationContainer.CurrentLanguage;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Color);
            GUILayout.Label(SettingsSystem.CurrentSettings.PlayerName, TempColorLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Red, _smallOption);
            TempColor.r = GUILayout.HorizontalScrollbar(TempColor.r, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Green, _smallOption);
            TempColor.g = GUILayout.HorizontalScrollbar(TempColor.g, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Blue, _smallOption);
            TempColor.b = GUILayout.HorizontalScrollbar(TempColor.b, 0, 0, 1);
            GUILayout.EndHorizontal();
            TempColorLabelStyle.active.textColor = TempColor;
            TempColorLabelStyle.normal.textColor = TempColor;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Random, ButtonStyle))
                TempColor = PlayerColorSystem.GenerateRandomColor();
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Set, ButtonStyle))
            {
                StatusWindow.Singleton.ColorEventHandled = false;
                SettingsSystem.CurrentSettings.PlayerColor = TempColor;
                SettingsSystem.SaveSettings();
                if (MainSystem.NetworkState == ClientState.Running)
                    PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.ResetDisclaimer))
            {
                SettingsSystem.CurrentSettings.DisclaimerAccepted = false;
                SettingsSystem.SaveSettings();
            }
            var settingInterpolation = GUILayout.Toggle(SettingsSystem.CurrentSettings.InterpolationEnabled, LocalizationContainer.OptionsWindowText.Interpolation, ButtonStyle);
            if (settingInterpolation != SettingsSystem.CurrentSettings.InterpolationEnabled)
            {
                SettingsSystem.CurrentSettings.InterpolationEnabled = settingInterpolation;
                SettingsSystem.SaveSettings();
            }
            var preciseSurfacePositioning = GUILayout.Toggle(SettingsSystem.CurrentSettings.PreciseSurfacePositioning, LocalizationContainer.OptionsWindowText.PreciseSurfacePositioning, ButtonStyle);
            if (preciseSurfacePositioning != SettingsSystem.CurrentSettings.PreciseSurfacePositioning)
            {
                SettingsSystem.CurrentSettings.PreciseSurfacePositioning = preciseSurfacePositioning;
                SettingsSystem.SaveSettings();
            }
            GUILayout.Space(10);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl))
                ModSystem.Singleton.GenerateModControlFile(false);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl + " + SHA"))
                ModSystem.Singleton.GenerateModControlFile(true);
            UniverseConverterWindow.Singleton.Display = GUILayout.Toggle(UniverseConverterWindow.Singleton.Display, LocalizationContainer.OptionsWindowText.GenerateUniverse, ButtonStyle);
            GUILayout.Space(10);
#if DEBUG
            DrawAdvancedDebugOptions();
#endif
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawAdvancedDebugOptions()
        {
            if (GUILayout.Button("Check Common.dll stock parts"))
                ModSystem.Singleton.CheckCommonStockParts();
            GUILayout.Space(10);

            ShowAdvancedNetworkFields = GUILayout.Toggle(ShowAdvancedNetworkFields, "Advanced network fields", ButtonStyle);
            if (ShowAdvancedNetworkFields)
            {
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label("Cannot change values while connected");
                }

                GUILayout.Label($"MTU: {NetworkMain.Config.MaximumTransmissionUnit}");
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
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Random", ButtonStyle)) NetworkMain.RandomizeBadConnectionValues();
                    if (GUILayout.Button("Reset", ButtonStyle)) NetworkMain.ResetBadConnectionValues();
                    GUILayout.EndHorizontal();
                }

                GUILayout.Label($"NTP time offset: {LunaTime.SimulatedMsTimeOffset:F1}ms");
                LunaTime.SimulatedMsTimeOffset = (float)Math.Round(GUILayout.HorizontalScrollbar(LunaTime.SimulatedMsTimeOffset, 0, -2500, 2500), 3);
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label("Cannot change values while connected");
                }

                GUILayout.Label($"Packet loss: {NetworkMain.Config.SimulatedLoss * 100:F1}%");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedLoss = (float)Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedLoss, 0, 0, 1), 3);
                GUILayout.Label($"Packet duplication: {NetworkMain.Config.SimulatedDuplicatesChance * 100:F1}%");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedDuplicatesChance = (float)Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedDuplicatesChance, 0, 0, 1), 3);
                GUILayout.Label($"Max random latency: {NetworkMain.Config.SimulatedRandomLatency * 1000:F1} ms");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedRandomLatency = (float)Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedRandomLatency, 0, 0, 3), 4);
                GUILayout.Label($"Min latency: {NetworkMain.Config.SimulatedMinimumLatency * 1000:F1} ms");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                    NetworkMain.Config.SimulatedMinimumLatency = (float)Math.Round(GUILayout.HorizontalScrollbar(NetworkMain.Config.SimulatedMinimumLatency, 0, 0, 3), 4);
            }
        }
    }
}
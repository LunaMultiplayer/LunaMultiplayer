using Lidgren.Network;
using LunaClient.Localization;
using LunaClient.Network;
using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Windows.Status;
using LunaCommon.Enums;
using LunaCommon.Time;
using System;
using UnityEngine;

namespace LunaClient.Windows.Options
{
    public partial class OptionsWindow
    {
        public override void DrawWindowContent(int windowId)
        {
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
            GUILayout.Label(SettingsSystem.CurrentSettings.PlayerName, _tempColorLabelStyle);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Red, _smallOption);
            _tempColor.r = GUILayout.HorizontalScrollbar(_tempColor.r, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Green, _smallOption);
            _tempColor.g = GUILayout.HorizontalScrollbar(_tempColor.g, 0, 0, 1);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Blue, _smallOption);
            _tempColor.b = GUILayout.HorizontalScrollbar(_tempColor.b, 0, 0, 1);
            GUILayout.EndHorizontal();
            _tempColorLabelStyle.active.textColor = _tempColor;
            _tempColorLabelStyle.normal.textColor = _tempColor;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Random, ButtonStyle))
                _tempColor = PlayerColorSystem.GenerateRandomColor();
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Set, ButtonStyle))
            {
                StatusWindow.Singleton.ColorEventHandled = false;
                SettingsSystem.CurrentSettings.PlayerColor = _tempColor;
                SettingsSystem.SaveSettings();
                if (MainSystem.NetworkState == ClientState.Running)
                    PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            var settingInterpolator = GUILayout.Toggle(SettingsSystem.CurrentSettings.PositionInterpolation, "Use interpolation", ButtonStyle);
            if (settingInterpolator != SettingsSystem.CurrentSettings.PositionInterpolation)
            {
                SettingsSystem.CurrentSettings.PositionInterpolation = settingInterpolator;
                SettingsSystem.SaveSettings();
            }
            if (SettingsSystem.CurrentSettings.PositionInterpolation)
            {
                GUILayout.Label($"Interpolation offset: {SettingsSystem.CurrentSettings.InterpolationOffset * 1000:F0} ms");
                var interpolationOffset = Math.Round(GUILayout.HorizontalScrollbar((float)SettingsSystem.CurrentSettings.InterpolationOffset, 0, 0, 3), 2);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (interpolationOffset != SettingsSystem.CurrentSettings.InterpolationOffset)
                {
                    SettingsSystem.CurrentSettings.InterpolationOffset = interpolationOffset;
                    SettingsSystem.SaveSettings();
                }
            }
            GUILayout.Space(10);

            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl))
                ModSystem.Singleton.GenerateModControlFile(false);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl + " + SHA"))
                ModSystem.Singleton.GenerateModControlFile(true);
            _displayUniverseConverterDialog = GUILayout.Toggle(_displayUniverseConverterDialog, LocalizationContainer.OptionsWindowText.GenerateUniverse, ButtonStyle);
            GUILayout.Space(10);

            DrawNetworkSettings();
#if DEBUG
            DrawAdvancedDebugOptions();
#endif
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private void DrawNetworkSettings()
        {
            _showAdvancedNetworkFields = GUILayout.Toggle(_showAdvancedNetworkFields, LocalizationContainer.OptionsWindowText.NetworkSettings, ButtonStyle);
            if (_showAdvancedNetworkFields)
            {
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label(LocalizationContainer.OptionsWindowText.CannotChangeWhileConnected);
                }

                GUILayout.Label($"MTU (Default: {NetPeerConfiguration.kDefaultMTU}): {NetworkMain.Config.MaximumTransmissionUnit}");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    if (NetworkMain.ClientConnection.Status != NetPeerStatus.NotRunning)
                    {
                        if (GUILayout.Button("Reset network"))
                            NetworkMain.ResetNetworkSystem();
                    }
                    else
                    {
                        var mtuValue = (int)Math.Round(GUILayout.HorizontalScrollbar(SettingsSystem.CurrentSettings.Mtu, 0, 1, NetworkMain.MaxMtuSize));
                        if (mtuValue != SettingsSystem.CurrentSettings.Mtu)
                        {
                            NetworkMain.Config.MaximumTransmissionUnit = SettingsSystem.CurrentSettings.Mtu = mtuValue;
                            SettingsSystem.SaveSettings();
                        }

                        var autoExpandValue = GUILayout.Toggle(SettingsSystem.CurrentSettings.AutoExpandMtu, "Auto expand MTU");
                        if (autoExpandValue != SettingsSystem.CurrentSettings.AutoExpandMtu)
                        {
                            NetworkMain.Config.AutoExpandMTU = SettingsSystem.CurrentSettings.AutoExpandMtu = autoExpandValue;
                            SettingsSystem.SaveSettings();
                        }
                    }
                }

                GUILayout.Label(_infiniteTimeout ? "Timeout (Default: 15): ∞" : $"Timeout (Default: 15): {NetworkMain.Config.ConnectionTimeout}s");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    _infiniteTimeout = SettingsSystem.CurrentSettings.Timeout == float.MaxValue;

                    GUI.enabled = !_infiniteTimeout;
                    var newTimeoutVal = (int)Math.Round(GUILayout.HorizontalScrollbar(SettingsSystem.CurrentSettings.Timeout, 0, NetworkMain.Config.PingInterval, 120));
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (newTimeoutVal != SettingsSystem.CurrentSettings.Timeout)
                    {
                        NetworkMain.Config.ConnectionTimeout = SettingsSystem.CurrentSettings.Timeout = newTimeoutVal;
                        SettingsSystem.SaveSettings();
                    }
                    GUI.enabled = true;

                    _infiniteTimeout = GUILayout.Toggle(_infiniteTimeout, "∞", "toggle");
                    if (_infiniteTimeout)
                    {
                        NetworkMain.Config.ConnectionTimeout = SettingsSystem.CurrentSettings.Timeout = float.MaxValue;
                        SettingsSystem.SaveSettings();
                    }
                }
            }
        }

        private void DrawAdvancedDebugOptions()
        {
            GUILayout.Label("Debug settings\n________________________________________");

            if (GUILayout.Button("Check Common.dll stock parts"))
                ModSystem.Singleton.CheckCommonStockParts();
            var settingIntegrator = GUILayout.Toggle(SettingsSystem.CurrentSettings.OverrideIntegrator, "Override flight integrator", ButtonStyle);
            if (settingIntegrator != SettingsSystem.CurrentSettings.OverrideIntegrator)
            {
                SettingsSystem.CurrentSettings.OverrideIntegrator = settingIntegrator;
                SettingsSystem.SaveSettings();
            }

            GUILayout.Space(10);

            _showBadNetworkSimulationFields = GUILayout.Toggle(_showBadNetworkSimulationFields, "Bad network simulation", ButtonStyle);
            if (_showBadNetworkSimulationFields)
            {
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Random", ButtonStyle)) NetworkMain.RandomizeBadConnectionValues();
                    if (GUILayout.Button("Reset", ButtonStyle)) NetworkMain.ResetBadConnectionValues();
                    GUILayout.EndHorizontal();
                }

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

            _showClockOffsetFields = GUILayout.Toggle(_showClockOffsetFields, "Clock offset simulation", ButtonStyle);
            if (_showClockOffsetFields)
            {
                GUILayout.Label($"Computer clock offset (minutes): {LunaComputerTime.SimulatedMinutesTimeOffset:F1}");
                LunaComputerTime.SimulatedMinutesTimeOffset = (float)Math.Round(GUILayout.HorizontalScrollbar(LunaComputerTime.SimulatedMinutesTimeOffset, 0, -15, 15), 3);

                GUILayout.Label($"NTP server time offset (milliseconds): {LunaNetworkTime.SimulatedMsTimeOffset:F1}");
                LunaNetworkTime.SimulatedMsTimeOffset = (float)Math.Round(GUILayout.HorizontalScrollbar(LunaNetworkTime.SimulatedMsTimeOffset, 0, -2500, 2500), 3);
            }
        }

        #region UniverseConverter

        private void DrawUniverseConverterDialog(int windowId)
        {
            //Always draw close button first
            DrawCloseButton(() => _displayUniverseConverterDialog = false, _universeConverterWindowRect);

            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            ScrollPos = GUILayout.BeginScrollView(ScrollPos, ScrollStyle);
            foreach (var saveFolder in Utilities.UniverseConverter.GetSavedNames())
                if (GUILayout.Button(saveFolder))
                    Utilities.UniverseConverter.GenerateUniverse(saveFolder);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        #endregion
    }
}

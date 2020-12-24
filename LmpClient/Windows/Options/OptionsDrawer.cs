using Lidgren.Network;
using LmpClient.Localization;
using LmpClient.Network;
using LmpClient.Systems.Mod;
using LmpClient.Systems.PlayerColorSys;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpClient.Windows.Status;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using UnityEngine;

// ReSharper disable CompareOfFloatsByEqualityOperator
namespace LmpClient.Windows.Options
{
    public partial class OptionsWindow
    {
        protected override void DrawWindowContent(int windowId)
        {
            //Player color
            GUILayout.BeginVertical();
            GUI.DragWindow(MoveRect);
            GUILayout.BeginHorizontal();
            GUILayout.Label(LocalizationContainer.OptionsWindowText.Language);
            if (GUILayout.Button(LocalizationContainer.GetCurrentLanguageAsText()))
            {
                LocalizationContainer.LoadLanguage(LocalizationContainer.GetNextLanguage());
                SettingsSystem.CurrentSettings.Language = LocalizationContainer.CurrentLanguage;
                SettingsSystem.SaveSettings();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label(LocalizationContainer.OptionsWindowText.Color);

            GUILayout.BeginHorizontal(Skin.box, GUILayout.ExpandWidth(true));
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

            _tempColorLabelStyle.fontStyle = FontStyle.Bold;
            _tempColorLabelStyle.fontSize = 40;
            _tempColorLabelStyle.alignment = TextAnchor.MiddleCenter;
            _tempColorLabelStyle.active.textColor = _tempColor;
            _tempColorLabelStyle.normal.textColor = _tempColor;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Random))
            {
                _tempColor = PlayerColorSystem.GenerateRandomColor();
            }
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.Set))
            {
                StatusWindow.Singleton.ColorEventHandled = false;
                SettingsSystem.CurrentSettings.PlayerColor = _tempColor;
                SettingsSystem.SaveSettings();
                if (MainSystem.NetworkState == ClientState.Running)
                    PlayerColorSystem.Singleton.MessageSender.SendPlayerColorToServer();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl))
                ModSystem.Singleton.GenerateModControlFile(false);
            if (GUILayout.Button(LocalizationContainer.OptionsWindowText.GenerateLmpModControl + " + SHA"))
                ModSystem.Singleton.GenerateModControlFile(true);
            _displayUniverseConverterDialog = GUILayout.Toggle(_displayUniverseConverterDialog, LocalizationContainer.OptionsWindowText.GenerateUniverse, ToggleButtonStyle);
            GUILayout.Space(10);

            DrawGeneralSettings();
            DrawNetworkSettings();
#if DEBUG
            DrawAdvancedDebugOptions();
#endif
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

        private static void DrawGeneralSettings()
        {
            _showGeneralSettings = GUILayout.Toggle(_showGeneralSettings, LocalizationContainer.OptionsWindowText.GeneralSettings, ToggleButtonStyle);
            if (_showGeneralSettings)
            {
                var settingSyncCheck = GUILayout.Toggle(SettingsSystem.CurrentSettings.IgnoreSyncChecks, LocalizationContainer.OptionsWindowText.IgnoreSyncChecks);
                if (settingSyncCheck != SettingsSystem.CurrentSettings.IgnoreSyncChecks)
                {
                    SettingsSystem.CurrentSettings.IgnoreSyncChecks = settingSyncCheck;
                    SettingsSystem.SaveSettings();
                }
            }
        }

        private static void DrawNetworkSettings()
        {
            _showAdvancedNetworkSettings = GUILayout.Toggle(_showAdvancedNetworkSettings, LocalizationContainer.OptionsWindowText.NetworkSettings, ToggleButtonStyle);
            if (_showAdvancedNetworkSettings)
            {
                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label(LocalizationContainer.OptionsWindowText.CannotChangeWhileConnected);
                }

                GUILayout.Label($"{LocalizationContainer.OptionsWindowText.Mtu} {NetworkMain.Config.MaximumTransmissionUnit}");
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    if (NetworkMain.ClientConnection.Status != NetPeerStatus.NotRunning)
                    {
                        if (GUILayout.Button(LocalizationContainer.OptionsWindowText.ResetNetwork))
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

                        var autoExpandValue = GUILayout.Toggle(SettingsSystem.CurrentSettings.AutoExpandMtu, LocalizationContainer.OptionsWindowText.AutoExpandMtu);
                        if (autoExpandValue != SettingsSystem.CurrentSettings.AutoExpandMtu)
                        {
                            NetworkMain.Config.AutoExpandMTU = SettingsSystem.CurrentSettings.AutoExpandMtu = autoExpandValue;
                            SettingsSystem.SaveSettings();
                        }
                    }
                }

                GUILayout.Label(SettingsSystem.CurrentSettings.TimeoutSeconds == float.MaxValue
                    ? $"{LocalizationContainer.OptionsWindowText.ConnectionTimeout} ∞"
                    : $"{LocalizationContainer.OptionsWindowText.ConnectionTimeout} {NetworkMain.Config.ConnectionTimeout} sec");

                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    _infiniteTimeout = SettingsSystem.CurrentSettings.TimeoutSeconds == float.MaxValue;

                    GUI.enabled = !_infiniteTimeout;
                    var newTimeoutVal = (int)Math.Round(GUILayout.HorizontalScrollbar(SettingsSystem.CurrentSettings.TimeoutSeconds, 0, NetworkMain.Config.PingInterval, 120));
                    if (newTimeoutVal != SettingsSystem.CurrentSettings.TimeoutSeconds)
                    {
                        NetworkMain.Config.ConnectionTimeout = SettingsSystem.CurrentSettings.TimeoutSeconds = newTimeoutVal;
                        SettingsSystem.SaveSettings();
                    }
                    GUI.enabled = true;

                    _infiniteTimeout = GUILayout.Toggle(_infiniteTimeout, "∞");
                    if (_infiniteTimeout)
                    {
                        NetworkMain.Config.ConnectionTimeout = SettingsSystem.CurrentSettings.TimeoutSeconds = float.MaxValue;
                        SettingsSystem.SaveSettings();
                    }
                }
            }
        }

        private static void DrawAdvancedDebugOptions()
        {
            GUILayout.Label("Debug settings");

            if (GUILayout.Button("Check Common.dll stock parts"))
                ModSystem.Singleton.CheckCommonStockParts();
            if (GUILayout.Button("Regenerate translation files"))
                LocalizationContainer.RegenerateTranslations();

            GUILayout.Space(10);

            _showBadNetworkSimulationSettings = GUILayout.Toggle(_showBadNetworkSimulationSettings, "Bad network simulation", ToggleButtonStyle);
            if (_showBadNetworkSimulationSettings)
            {
                if (MainSystem.NetworkState <= ClientState.Disconnected)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Random")) NetworkMain.RandomizeBadConnectionValues();
                    if (GUILayout.Button("Reset")) NetworkMain.ResetBadConnectionValues();
                    GUILayout.EndHorizontal();
                }

                if (MainSystem.NetworkState > ClientState.Disconnected)
                {
                    GUILayout.Label("Cannot change values while connected");
                }

#if DEBUG
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
#endif
            }

            _showClockOffsetSettings = GUILayout.Toggle(_showClockOffsetSettings, "Clock offset simulation", ToggleButtonStyle);
            if (_showClockOffsetSettings)
            {
                GUILayout.Label($"Computer clock offset: {LunaComputerTime.SimulatedMinutesTimeOffset:F1} min");
                LunaComputerTime.SimulatedMinutesTimeOffset = (float)Math.Round(GUILayout.HorizontalScrollbar(LunaComputerTime.SimulatedMinutesTimeOffset, 0, -15, 15), 3);

                GUILayout.Label($"NTP server time offset: {LunaNetworkTime.SimulatedMsTimeOffset:F1} ms");
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
            ScrollPos = GUILayout.BeginScrollView(ScrollPos);
            foreach (var saveFolder in UniverseConverter.GetSavedNames())
                if (GUILayout.Button(saveFolder))
                    UniverseConverter.GenerateUniverse(saveFolder);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        #endregion
    }
}

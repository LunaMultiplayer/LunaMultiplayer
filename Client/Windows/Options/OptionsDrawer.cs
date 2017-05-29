using LunaClient.Systems.Mod;
using LunaClient.Systems.PlayerColorSys;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Toolbar;
using LunaClient.Utilities;
using LunaClient.Windows.Status;
using LunaClient.Windows.UniverseConverter;
using LunaCommon.Enums;
using System;
using LunaClient.Systems;
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
                NewCacheSize = SettingsSystem.CurrentSettings.CacheSize.ToString();
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
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
                if (SystemsContainer.Get<MainSystem>().NetworkState == ClientState.Running)
                    SystemsContainer.Get<PlayerColorSystem>().MessageSender.SendPlayerColorToServer();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            //Cache
            GUILayout.Label("Cache size");
            GUILayout.Label($"Current size: {Math.Round(UniverseSyncCache.CurrentCacheSize / (float)(1024 * 1024), 3)} MB.");
            GUILayout.Label($"Max size: {SettingsSystem.CurrentSettings.CacheSize} MB.");
            NewCacheSize = GUILayout.TextArea(NewCacheSize);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set", ButtonStyle))
            {
                if (int.TryParse(NewCacheSize, out var tempCacheSize))
                {
                    if (tempCacheSize < 1)
                    {
                        tempCacheSize = 1;
                        NewCacheSize = tempCacheSize.ToString();
                    }
                    if (tempCacheSize > 1000)
                    {
                        tempCacheSize = 1000;
                        NewCacheSize = tempCacheSize.ToString();
                    }
                    SettingsSystem.CurrentSettings.CacheSize = tempCacheSize;
                    SystemsContainer.Get<SettingsSystem>().SaveSettings();
                }
                else
                {
                    NewCacheSize = SettingsSystem.CurrentSettings.CacheSize.ToString();
                }
            }
            if (GUILayout.Button("Expire cache"))
                UniverseSyncCache.ExpireCache();
            if (GUILayout.Button("Delete cache"))
                UniverseSyncCache.DeleteCache();
            GUILayout.EndHorizontal();
            //Key bindings
            GUILayout.Space(10);
            var chatDescription = $"Set chat key (current: {SettingsSystem.CurrentSettings.ChatKey})";
            if (SettingChat)
            {
                chatDescription = "Setting chat key (click to cancel)...";
                if (Event.current.isKey)
                    if (Event.current.keyCode != KeyCode.Escape)
                    {
                        SettingsSystem.CurrentSettings.ChatKey = Event.current.keyCode;
                        SystemsContainer.Get<SettingsSystem>().SaveSettings();
                        SettingChat = false;
                    }
                    else
                    {
                        SettingChat = false;
                    }
            }
            if (GUILayout.Button(chatDescription))
                SettingChat = !SettingChat;
            GUILayout.Space(10);
            GUILayout.Label("Generate a server LMPModControl:");
            if (GUILayout.Button("Generate blacklist LMPModControl.txt"))
                SystemsContainer.Get<ModSystem>().GenerateModControlFile(false);
            if (GUILayout.Button("Generate whitelist LMPModControl.txt"))
                SystemsContainer.Get<ModSystem>().GenerateModControlFile(true);
            WindowsContainer.Get<UniverseConverterWindow>().Display = GUILayout.Toggle(WindowsContainer.Get<UniverseConverterWindow>().Display, "Generate Universe from saved game", ButtonStyle);
            if (GUILayout.Button("Reset disclaimer"))
            {
                SettingsSystem.CurrentSettings.DisclaimerAccepted = false;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
            }
            var settingCompression = GUILayout.Toggle(SettingsSystem.CurrentSettings.CompressionEnabled, "Enable compression", ButtonStyle);
            if (settingCompression != SettingsSystem.CurrentSettings.CompressionEnabled)
            {
                SettingsSystem.CurrentSettings.CompressionEnabled = settingCompression;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
            }
            var settingInterpolation = GUILayout.Toggle(SettingsSystem.CurrentSettings.InterpolationEnabled, "Enable interpolation", ButtonStyle);
            if (settingInterpolation != SettingsSystem.CurrentSettings.InterpolationEnabled)
            {
                SettingsSystem.CurrentSettings.InterpolationEnabled = settingInterpolation;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
            }
            var positionFudge = GUILayout.Toggle(SettingsSystem.CurrentSettings.PositionFudgeEnable, "Enable position fudge", ButtonStyle);
            if (positionFudge != SettingsSystem.CurrentSettings.PositionFudgeEnable)
            {
                SettingsSystem.CurrentSettings.PositionFudgeEnable = positionFudge;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
            }
            var packOtherVessels = GUILayout.Toggle(SettingsSystem.CurrentSettings.PackOtherControlledVessels, "Pack other vessels", ButtonStyle);
            if (packOtherVessels != SettingsSystem.CurrentSettings.PackOtherControlledVessels)
            {
                SettingsSystem.CurrentSettings.PackOtherControlledVessels = packOtherVessels;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Toolbar:", SmallOption);
            if (GUILayout.Button(ToolbarMode, ButtonStyle))
            {
                var newSetting = (int)SettingsSystem.CurrentSettings.ToolbarType + 1;
                //Overflow to 0
                if (!Enum.IsDefined(typeof(LmpToolbarType), newSetting))
                    newSetting = 0;
                SettingsSystem.CurrentSettings.ToolbarType = (LmpToolbarType)newSetting;
                SystemsContainer.Get<SettingsSystem>().SaveSettings();
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
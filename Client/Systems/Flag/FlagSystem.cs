using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.Flag
{
    public class FlagSystem : MessageSystem<FlagSystem, FlagMessageSender, FlagMessageHandler>
    {
        #region Fields

        public FlagEvents FlagEvents { get; } = new FlagEvents();
        public static string FlagPath { get; } = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiPlayer", "Flags");
        public ConcurrentDictionary<string, ExtendedFlagInfo> ServerFlags { get; } = new ConcurrentDictionary<string, ExtendedFlagInfo>();
        private bool FlagSystemReady => Enabled && HighLogic.CurrentGame?.flagURL != null;

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(FlagSystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onFlagSelect.Add(FlagEvents.OnFlagSelect);
            GameEvents.onMissionFlagSelect.Add(FlagEvents.OnMissionFlagSelect);
            SetupRoutine(new RoutineDefinition(5000, RoutineExecution.Update, HandleFlags));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ServerFlags.Clear();
            GameEvents.onFlagSelect.Remove(FlagEvents.OnFlagSelect);
            GameEvents.onMissionFlagSelect.Remove(FlagEvents.OnMissionFlagSelect);
        }

        #endregion

        #region Update methods

        private void HandleFlags()
        {
            if (FlagSystemReady)
            {
                foreach (var flag in ServerFlags.Where(v => !v.Value.Loaded))
                {
                    HandleFlag(flag.Value);
                    flag.Value.Loaded = true;
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Send our flag to the server
        /// </summary>
        public void SendCurrentFlag()
        {
            //If the flag does not come from the LMP folder then skip this
            if (!SettingsSystem.CurrentSettings.SelectedFlag.Contains("LunaMultiPlayer/Flags/"))
                return;

            var flagName = SettingsSystem.CurrentSettings.SelectedFlag.Substring("LunaMultiPlayer/Flags/".Length);
            var fullFlagPath = CommonUtil.CombinePaths(FlagPath, flagName);

            if (!File.Exists(fullFlagPath)) return;
            
            MessageSender.SendMessage(MessageSender.GetFlagMessageData(flagName, fullFlagPath));
        }

        public static bool FlagFileExists()
        {
            if (string.IsNullOrEmpty(SettingsSystem.CurrentSettings.SelectedFlag) || !SettingsSystem.CurrentSettings.SelectedFlag.Contains("/"))
                return false;

            var flagName = SettingsSystem.CurrentSettings.SelectedFlag
                .Substring(SettingsSystem.CurrentSettings.SelectedFlag.LastIndexOf("/", StringComparison.Ordinal) + 1) + ".png";

            var fullFlagPath = CommonUtil.CombinePaths(FlagPath, flagName);

            return File.Exists(fullFlagPath);
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Here we handle an unloaded flag and we load it into the game
        /// </summary>
        private static void HandleFlag(ExtendedFlagInfo flagInfo)
        {
            if (HighLogic.CurrentGame.flagURL.Contains("LunaMultiPlayer/Flags/"))
            {
                var currentFlagName = HighLogic.CurrentGame.flagURL.Substring("LunaMultiPlayer/Flags/".Length);
                //If the flag name is the same as ours just skip it as otherwise we would overwrite ours
                if (currentFlagName == flagInfo.FlagName)
                    return;
            }

            var flagTexture = new Texture2D(4, 4);
            if (flagTexture.LoadImage(flagInfo.FlagData))
            {
                //Flags have names like: Squad/Flags/default or LunaMultiplayer/Flags/coolflag
                flagTexture.name = "LunaMultiPlayer/Flags/" + Path.GetFileNameWithoutExtension(flagInfo.FlagName);
                File.WriteAllBytes(flagInfo.FlagPath, flagInfo.FlagData);

                var textureInfo = new GameDatabase.TextureInfo(null, flagTexture, false, true, false)
                {
                    name = flagTexture.name
                };

                var textureExists = GameDatabase.Instance.databaseTexture.Any(t => t.name == textureInfo.name);

                if (!textureExists)
                    GameDatabase.Instance.databaseTexture.Add(textureInfo);
                else
                    GameDatabase.Instance.ReplaceTexture(textureInfo.name, textureInfo);

                LunaLog.Log($"[LMP]: Loaded {flagTexture.name}");
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to load flag {flagInfo.FlagName}");
            }
        }

        #endregion
    }
}
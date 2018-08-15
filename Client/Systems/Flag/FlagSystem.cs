using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Flags;
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
        public static string LmpFlagPath { get; } = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Flags");
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
            if (DefaultFlags.DefaultFlagList.Contains(SettingsSystem.CurrentSettings.SelectedFlag))
                return;

            var textureInfo = GameDatabase.Instance.GetTextureInfo(SettingsSystem.CurrentSettings.SelectedFlag);
            if (textureInfo != null)
            {
                MessageSender.SendMessage(MessageSender.GetFlagMessageData(SettingsSystem.CurrentSettings.SelectedFlag, textureInfo.texture.GetRawTextureData()));
            }
        }

        public bool FlagExists(string flagUrl)
        {
            return GameDatabase.Instance.ExistsTexture($"{flagUrl}");
        }

        public void SaveCurrentFlags()
        {
            foreach (var textureInfo in GameDatabase.Instance.GetAllTexturesInFolderType("Flag", true).Where(f=> !DefaultFlags.DefaultFlagList.Contains(f.name)))
            {
                try
                {
                    File.WriteAllBytes(Path.GetFileName(textureInfo.name) + ".png", textureInfo.texture.GetRawTextureData());
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"Error while trying to save flag {textureInfo.name}: {e}");
                }
            }
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Here we handle an unloaded flag and we load it into the game
        /// </summary>
        private void HandleFlag(ExtendedFlagInfo flagInfo)
        {
            //We have a flag with the same name!
            if (FlagExists(flagInfo.FlagName))
                return;

            var flagTexture = new Texture2D(4, 4);
            if (flagTexture.LoadImage(flagInfo.FlagData))
            {
                //Flags have names like: Squad/Flags/default
                flagTexture.name = flagInfo.FlagName;
                var textureInfo = new GameDatabase.TextureInfo(null, flagTexture, false, true, false)
                {
                    name = flagTexture.name
                };

                GameDatabase.Instance.databaseTexture.Add(textureInfo);
                LunaLog.Log($"[LMP]: Loaded flag {flagTexture.name}");
            }
            else
            {
                LunaLog.LogError($"[LMP]: Failed to load flag {flagInfo.FlagName}");
            }
        }

        #endregion
    }
}
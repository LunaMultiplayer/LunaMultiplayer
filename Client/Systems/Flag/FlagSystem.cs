using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Message.Data.Flag;
using UnityEngine;

namespace LunaClient.Systems.Flag
{
    public class FlagSystem : MessageSystem<FlagSystem, FlagMessageSender, FlagMessageHandler>
    {
        #region Base overrides

        public override void Update()
        {
            base.Update();
            if (Enabled && SyncComplete && (HighLogic.CurrentGame != null) && (HighLogic.CurrentGame.flagURL != null))
            {
                if (FlagChangeEvent)
                {
                    FlagChangeEvent = false;
                    HandleFlagChangeEvent();
                }

                FlagRespondMessage flagRespond;
                while (NewFlags.TryDequeue(out flagRespond))
                    HandleFlagRespondMessage(flagRespond);
            }
        }

        #endregion

        #region Public methods

        public void SendFlagList()
        {
            var flags = Directory.GetFiles(FlagPath);
            var shas = new string[flags.Length];

            for (var i = 0; i < flags.Length; i++)
            {
                shas[i] = Common.CalculateSha256Hash(flags[i]);
                flags[i] = Path.GetFileName(flags[i]);
            }

            MessageSender.SendMessage(new FlagListMsgData
            {
                PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                FlagShaSums = shas,
                FlagFileNames = flags
            });
        }

        #endregion

        #region Fields

        public bool FlagChangeEvent { get; set; }
        public bool SyncComplete { get; set; }
        public string FlagPath { get; } = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", "LunaMultiPlayer", "Flags");
        public Dictionary<string, FlagInfo> ServerFlags { get; } = new Dictionary<string, FlagInfo>();
        public ConcurrentQueue<FlagRespondMessage> NewFlags { get; } = new ConcurrentQueue<FlagRespondMessage>();

        #endregion

        #region Private methods

        private void HandleFlagChangeEvent()
        {
            var flagUrl = HighLogic.CurrentGame.flagURL;

            //If it's not a LMP flag don't sync it.
            if (!flagUrl.ToLower().StartsWith("lunamultiplayer/flags/")) return;

            var flagName = flagUrl.Substring("LunaMultiPlayer/Flags/".Length);

            //If the flag is owned by someone else don't sync it
            if (ServerFlags.ContainsKey(flagName) &&
                (ServerFlags[flagName].Owner != SettingsSystem.CurrentSettings.PlayerName)) return;

            var flagFiles = Directory.GetFiles(FlagPath, "*", SearchOption.TopDirectoryOnly);
            var flagFile = flagFiles.FirstOrDefault(f => string.Equals(flagName, Path.GetFileNameWithoutExtension(f),
                StringComparison.CurrentCultureIgnoreCase));

            //Sanity check to make sure we found the file
            if (!string.IsNullOrEmpty(flagFile) && File.Exists(flagFile))
            {
                var shaSum = Common.CalculateSha256Hash(flagFile);

                //Don't send the flag when the SHA sum already matches
                if (ServerFlags.ContainsKey(flagName) && (ServerFlags[flagName].ShaSum == shaSum)) return;

                LunaLog.Debug("Uploading " + Path.GetFileName(flagFile));
                MessageSender.SendMessage(new FlagUploadMsgData
                {
                    PlayerName = SettingsSystem.CurrentSettings.PlayerName,
                    FlagName = Path.GetFileName(flagFile),
                    FlagData = File.ReadAllBytes(flagFile)
                });
                var fi = new FlagInfo
                {
                    Owner = SettingsSystem.CurrentSettings.PlayerName,
                    ShaSum = Common.CalculateSha256Hash(flagFile)
                };

                ServerFlags[flagName] = fi;
            }
        }

        private void HandleFlagRespondMessage(FlagRespondMessage flagRespondMessage)
        {
            ServerFlags[flagRespondMessage.FlagName] = flagRespondMessage.FlagInfo;
            var flagFile = CommonUtil.CombinePaths(FlagPath, flagRespondMessage.FlagName);
            var flagTexture = new Texture2D(4, 4);

            if (flagTexture.LoadImage(flagRespondMessage.FlagData))
            {
                flagTexture.name = "LunaMultiPlayer/Flags/" +
                                   Path.GetFileNameWithoutExtension(flagRespondMessage.FlagName);
                File.WriteAllBytes(flagFile, flagRespondMessage.FlagData);

                var textureInfo = new GameDatabase.TextureInfo(null, flagTexture, false, true, false)
                {
                    name = flagTexture.name
                };

                var containsTexture = GameDatabase.Instance.databaseTexture.Any(t => t.name == textureInfo.name);

                if (!containsTexture)
                    GameDatabase.Instance.databaseTexture.Add(textureInfo);
                else
                    GameDatabase.Instance.ReplaceTexture(textureInfo.name, textureInfo);

                LunaLog.Debug("Loaded " + flagTexture.name);
            }
            else
            {
                LunaLog.Debug("Failed to load flag " + flagRespondMessage.FlagName);
            }
        }

        #endregion
    }
}
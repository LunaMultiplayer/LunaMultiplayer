using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem : MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        #region Fields and properties

        private static readonly string SaveFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "saves", "LunaMultiplayer");

        private static DateTime _lastRequest = DateTime.MinValue;

        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>> CraftInfo { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>> CraftDownloaded { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>>();

        public List<CraftEntry> OwnCrafts { get; } = new List<CraftEntry>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(CraftLibrarySystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            RefreshOwnCrafts();
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CraftInfo.Clear();
            CraftDownloaded.Clear();
        }

        #endregion

        #region Public methods

        public void RefreshOwnCrafts()
        {
            var vabFolder = CommonUtil.CombinePaths(SaveFolder, "Ships", "Vab");
            foreach (var file in Directory.GetFiles(vabFolder))
            {
                var data = File.ReadAllBytes(file);
                OwnCrafts.Add(new CraftEntry
                {
                    CraftName = Path.GetFileNameWithoutExtension(file),
                    CraftType = CraftType.Vab,
                    FolderName = SettingsSystem.CurrentSettings.PlayerName,
                    CraftData = data,
                    CraftNumBytes = data.Length
                });
            }

            var sphFolder = CommonUtil.CombinePaths(SaveFolder, "Ships", "Sph");
            foreach (var file in Directory.GetFiles(sphFolder))
            {
                var data = File.ReadAllBytes(file);
                OwnCrafts.Add(new CraftEntry
                {
                    CraftName = Path.GetFileNameWithoutExtension(file),
                    CraftType = CraftType.Sph,
                    FolderName = SettingsSystem.CurrentSettings.PlayerName,
                    CraftData = data,
                    CraftNumBytes = data.Length
                });
            }

            var subassemblyFolder = CommonUtil.CombinePaths(SaveFolder, "Subassemblies");
            foreach (var file in Directory.GetFiles(subassemblyFolder))
            {
                var data = File.ReadAllBytes(file);
                OwnCrafts.Add(new CraftEntry
                {
                    CraftName = Path.GetFileNameWithoutExtension(file),
                    CraftType = CraftType.Subassembly,
                    FolderName = SettingsSystem.CurrentSettings.PlayerName,
                    CraftData = data,
                    CraftNumBytes = data.Length
                });
            }
        }

        public void SaveCraftToDisk(CraftEntry craft)
        {
            string folder;
            switch (craft.CraftType)
            {
                case CraftType.Vab:
                    folder = CommonUtil.CombinePaths(SaveFolder, "Ships", "Vab");
                    break;
                case CraftType.Sph:
                    folder = CommonUtil.CombinePaths(SaveFolder, "Ships", "Sph");
                    break;
                case CraftType.Subassembly:
                    folder = CommonUtil.CombinePaths(SaveFolder, "Subassemblies");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var path = CommonUtil.CombinePaths(folder, craft.CraftName, ".craft");
            File.WriteAllBytes(path, craft.CraftData);

            ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.CraftSaved, 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public void SendCraft(CraftEntry craft)
        {
            if (DateTime.Now - _lastRequest > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs))
            {
                _lastRequest = DateTime.Now;
                MessageSender.SendCraftMsg(craft);
            }
            else
            {
                var msg = LocalizationContainer.ScreenText.CraftLibraryInterval.Replace("$1", 
                    TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs).TotalSeconds.ToString(CultureInfo.InvariantCulture));

                ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public void RequestCraft(CraftBasicEntry craft)
        {
            if (DateTime.Now - _lastRequest > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs))
            {
                _lastRequest = DateTime.Now;
                MessageSender.SendRequestCraftMsg(craft);
            }
            else
            {
                var msg = LocalizationContainer.ScreenText.CraftLibraryInterval.Replace("$1",
                    TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs).TotalSeconds.ToString(CultureInfo.InvariantCulture));

                ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        #endregion
    }
}

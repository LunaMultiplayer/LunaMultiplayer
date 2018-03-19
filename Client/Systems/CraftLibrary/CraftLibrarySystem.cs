using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaCommon.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace LunaClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem : MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        #region Fields and properties

        private static readonly string SaveFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "saves", "LunaMultiplayer");

        private static DateTime _lastuploadedCraft = DateTime.MinValue;

        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>> CraftInfo { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>> CraftDownloaded { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>>();

        public List<CraftEntry> OwnCrafts { get; } = new List<CraftEntry>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(CraftLibrarySystem);

        protected override bool ProcessMessagesInUnityThread => false;
        
        protected override void OnDisabled()
        {
            base.OnDisabled();
            CraftInfo.Clear();
            CraftDownloaded.Clear();
        }

        #endregion

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
    }
}

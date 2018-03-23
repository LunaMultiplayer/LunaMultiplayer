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
using System.Linq;

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

        public ConcurrentQueue<string> DownloadedCraftsNotification { get; } = new ConcurrentQueue<string>();
        public List<string> FoldersWithNewContent { get; } = new List<string>();
        public bool NewContent => FoldersWithNewContent.Any();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(CraftLibrarySystem);

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            RefreshOwnCrafts();
            MessageSender.SendRequestFoldersMsg();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, NotifyDownloadedCrafts));
        }

        private void NotifyDownloadedCrafts()
        {
            while (DownloadedCraftsNotification.TryDequeue(out var message))
                ScreenMessages.PostScreenMessage($"({message}) {LocalizationContainer.ScreenText.CraftSaved}", 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CraftInfo.Clear();
            CraftDownloaded.Clear();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Refreshes the list of our own crafts
        /// </summary>
        public void RefreshOwnCrafts()
        {
            var vabFolder = CommonUtil.CombinePaths(SaveFolder, "Ships", "VAB");
            if (Directory.Exists(vabFolder))
            {
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
            }

            var sphFolder = CommonUtil.CombinePaths(SaveFolder, "Ships", "SPH");
            if (Directory.Exists(sphFolder))
            {
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
            }

            var subassemblyFolder = CommonUtil.CombinePaths(SaveFolder, "Subassemblies");
            if (Directory.Exists(subassemblyFolder))
            {
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

        /// <summary>
        /// Saves a craft to the hard drive
        /// </summary>
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

            var path = CommonUtil.CombinePaths(folder, $"{craft.CraftName}.craft");
            File.WriteAllBytes(path, craft.CraftData);

            //Add it to the queue notification as we are in another thread
            DownloadedCraftsNotification.Enqueue(craft.CraftName);
        }

        /// <summary>
        /// Sends a craft to the server if possible
        /// </summary>
        public void SendCraft(CraftEntry craft)
        {
            if (DateTime.Now - _lastRequest > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs))
            {
                _lastRequest = DateTime.Now;
                MessageSender.SendCraftMsg(craft);
                ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.CraftUploaded, 10f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                var msg = LocalizationContainer.ScreenText.CraftLibraryInterval.Replace("$1", 
                    TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs).TotalSeconds.ToString(CultureInfo.InvariantCulture));

                ScreenMessages.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        /// <summary>
        /// Request a craft to the server if possible
        /// </summary>
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

        public void RequestCraftListIfNeeded(string selectedFolder)
        {
            if (FoldersWithNewContent.Contains(selectedFolder))
            {
                FoldersWithNewContent.Remove(selectedFolder);
                MessageSender.SendRequestCraftListMsg(selectedFolder);
                return;
            }

            if (CraftInfo.GetOrAdd(selectedFolder, new ConcurrentDictionary<string, CraftBasicEntry>()).Count == 0)
                MessageSender.SendRequestCraftListMsg(selectedFolder);
        }
    }
}

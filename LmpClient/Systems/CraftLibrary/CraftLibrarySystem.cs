using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.SettingsSys;
using LmpClient.Utilities;
using LmpCommon.Enums;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LmpClient.Systems.CraftLibrary
{
    public class CraftLibrarySystem : MessageSystem<CraftLibrarySystem, CraftLibraryMessageSender, CraftLibraryMessageHandler>
    {
        #region Fields and properties

        private static readonly string SaveFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "saves", "LunaMultiplayer");

        private static DateTime _lastRequest = DateTime.MinValue;
        private static readonly SemaphoreSlim _craftIoSemaphore = new SemaphoreSlim(1, 1);

        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>> CraftInfo { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftBasicEntry>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>> CraftDownloaded { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, CraftEntry>>();

        public List<CraftEntry> OwnCrafts { get; private set; } = new List<CraftEntry>();

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
                LunaScreenMsg.PostScreenMessage($"({message}) {LocalizationContainer.ScreenText.CraftSaved}", 5f, ScreenMessageStyle.UPPER_CENTER);
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
        /// Refreshes the list of our own crafts in the background
        /// </summary>
        public void RefreshOwnCrafts()
        {
            Task.Run(RefreshOwnCraftsAsync);
        }

        private async Task RefreshOwnCraftsAsync()
        {
            await _craftIoSemaphore.WaitAsync();
            try
            {
                var newOwnCrafts = new List<CraftEntry>();

                var vabFolder = CommonUtil.CombinePaths(SaveFolder, "Ships", "VAB");
                if (Directory.Exists(vabFolder))
                {
                    foreach (var file in Directory.GetFiles(vabFolder))
                    {
                        var data = File.ReadAllBytes(file);
                        newOwnCrafts.Add(new CraftEntry
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
                        newOwnCrafts.Add(new CraftEntry
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
                        newOwnCrafts.Add(new CraftEntry
                        {
                            CraftName = Path.GetFileNameWithoutExtension(file),
                            CraftType = CraftType.Subassembly,
                            FolderName = SettingsSystem.CurrentSettings.PlayerName,
                            CraftData = data,
                            CraftNumBytes = data.Length
                        });
                    }
                }

                OwnCrafts = newOwnCrafts;
            }
            catch (Exception ex)
            {
                LunaLog.LogError($"[LMP]: Error refreshing crafts: {ex.Message}");
            }
            finally
            {
                _craftIoSemaphore.Release();
            }
        }

        /// <summary>
        /// Saves a craft to the hard drive asynchronously
        /// </summary>
        public void SaveCraftToDisk(CraftEntry craft)
        {
            Task.Run(async () =>
            {
                await _craftIoSemaphore.WaitAsync();
                try
                {
                    string folder;
                    switch (craft.CraftType)
                    {
                        case CraftType.Vab:
                            folder = CommonUtil.CombinePaths(SaveFolder, "Ships", "VAB");
                            break;
                        case CraftType.Sph:
                            folder = CommonUtil.CombinePaths(SaveFolder, "Ships", "SPH");
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
                catch (Exception ex)
                {
                    LunaLog.LogError($"[LMP]: Error saving craft to disk: {ex.Message}");
                }
                finally
                {
                    _craftIoSemaphore.Release();
                }
            });
        }

        /// <summary>
        /// Sends a craft to the server if possible
        /// </summary>
        public void SendCraft(CraftEntry craft)
        {
            if (TimeUtil.IsInInterval(ref _lastRequest, SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs))
            {
                MessageSender.SendCraftMsg(craft);
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.CraftUploaded, 10f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                var msg = LocalizationContainer.ScreenText.CraftLibraryInterval.Replace("$1",
                    TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs).TotalSeconds.ToString(CultureInfo.InvariantCulture));

                LunaScreenMsg.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        /// <summary>
        /// Request a craft to the server if possible
        /// </summary>
        public void RequestCraft(CraftBasicEntry craft)
        {
            if (TimeUtil.IsInInterval(ref _lastRequest, SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs))
            {
                MessageSender.SendRequestCraftMsg(craft);
            }
            else
            {
                var msg = LocalizationContainer.ScreenText.CraftLibraryInterval.Replace("$1",
                    TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.MinCraftLibraryRequestIntervalMs).TotalSeconds.ToString(CultureInfo.InvariantCulture));

                LunaScreenMsg.PostScreenMessage(msg, 20f, ScreenMessageStyle.UPPER_CENTER);
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

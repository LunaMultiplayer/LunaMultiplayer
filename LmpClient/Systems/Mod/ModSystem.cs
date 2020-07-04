using Expansions;
using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Utilities;
using LmpCommon;
using LmpCommon.ModFile.Structure;
using LmpCommon.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LmpClient.Systems.Mod
{
    public class ModSystem : System<ModSystem>
    {
        #region Fields & properties

        public Dictionary<string, string> DllList { get; } = new Dictionary<string, string>();
        public bool ModControl { get; set; } = true;

        public ModControlStructure ModControlData { get; set; }
        public List<string> AllowedParts { get; set; } = new List<string>();
        public List<string> AllowedResources { get; set; } = new List<string>();

        public List<string> MissingExpansions { get; } = new List<string>();
        public List<ForbiddenDllFile> ForbiddenFilesFound { get; } = new List<ForbiddenDllFile>();
        public List<string> NonListedFilesFound { get; } = new List<string>();
        public List<MandatoryDllFile> MandatoryFilesNotFound { get; } = new List<MandatoryDllFile>();
        public List<MandatoryDllFile> MandatoryFilesDifferentSha { get; } = new List<MandatoryDllFile>();
        public List<MandatoryPart> MandatoryPartsNotFound { get; } = new List<MandatoryPart>();

        public ModFileHandler ModFileHandler { get; } = new ModFileHandler();

        private static readonly FieldInfo ExpansionsInfo = typeof(ExpansionsLoader).GetField("expansionsInfo", BindingFlags.NonPublic | BindingFlags.Static);

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ModSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            Clear();
        }

        public override int ExecutionOrder => int.MinValue + 1;

        #endregion

        #region Public methods

        public void Clear()
        {
            ModControl = true;

            AllowedParts.Clear();
            AllowedResources.Clear();
            MissingExpansions.Clear();
            ForbiddenFilesFound.Clear();
            NonListedFilesFound.Clear();
            MandatoryFilesNotFound.Clear();
            MandatoryFilesDifferentSha.Clear();
            MandatoryPartsNotFound.Clear();

            ModControlData = null;
        }

        public void BuildDllFileList()
        {
            DllList.Clear();

            foreach (var modFile in GetModFiles())
            {
                var fileHash = Common.CalculateSha256FileHash(modFile);
                DllList.Add(GetRelativePath(modFile), fileHash);
            }
        }

        public void GenerateModControlFile(bool appendSha)
        {
            var modCtrlStructure = new ModControlStructure
            {
                RequiredExpansions = GetInstalledExpansions()
            };

            modCtrlStructure.AllowedParts.AddRange(PartLoader.LoadedPartsList.Select(p => p.name));
            modCtrlStructure.AllowedResources.AddRange(PartResourceLibrary.Instance.resourceDefinitions.Cast<PartResourceDefinition>().Select(r => r.name));

            foreach (var modFile in GetModFiles())
            {
                modCtrlStructure.MandatoryPlugins.Add(new MandatoryDllFile
                {
                    FilePath = GetRelativePath(modFile),
                    Sha = appendSha ? Common.CalculateSha256FileHash(modFile) : string.Empty,
                    Text = $"{Path.GetFileNameWithoutExtension(modFile)}. Version: {FileVersionInfo.GetVersionInfo(modFile).FileVersion}"
                });
            }

            LunaXmlSerializer.WriteToXmlFile(modCtrlStructure, CommonUtil.CombinePaths(MainSystem.KspPath, "LMPModControl.xml"));
            LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.ModFileGenerated, 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public void CheckCommonStockParts()
        {
            var missingPartsCount = 0;
            var missingResourcesCount = 0;
            var modFile = new ModControlStructure();
            modFile.SetDefaultAllowedParts();
            modFile.SetDefaultAllowedResources();

            LunaLog.Log("[LMP]: Missing parts start");
            foreach (var part in PartLoader.LoadedPartsList.Where(p => !modFile.AllowedParts.Contains(p.name)))
            {
                missingPartsCount++;
                LunaLog.Log($"[LMP]: Missing part: '{part.name}'");
            }
            LunaLog.Log("[LMP]: Missing parts end");
            
            LunaLog.Log("[LMP]: Missing resources start");
            foreach (var resource in PartResourceLibrary.Instance.resourceDefinitions.Cast<PartResourceDefinition>().Select(r => r.name)
                .Where(r => !modFile.AllowedResources.Contains(r)))
            {
                missingResourcesCount++;
                LunaLog.Log($"[LMP]: Missing resource: '{resource}'");
            }
            LunaLog.Log("[LMP]: Missing resources end");

            if (missingPartsCount > 0 && missingResourcesCount <= 0)
            {
                LunaScreenMsg.PostScreenMessage($"{missingPartsCount} missing part(s) from Common.dll printed to log ({PartLoader.LoadedPartsList.Count} total)",
                    5f, ScreenMessageStyle.UPPER_CENTER);
            }
            else if (missingPartsCount <= 0 && missingResourcesCount <= 0)
            {
                LunaScreenMsg.PostScreenMessage("No missing parts/resources from Common.dll", 5f, ScreenMessageStyle.UPPER_CENTER);
            }
            else if (missingPartsCount <= 0 && missingResourcesCount > 0)
            {
                LunaScreenMsg.PostScreenMessage($"{missingResourcesCount} missing resources from Common.dll printed to log ({PartResourceLibrary.Instance.resourceDefinitions.Count} total)", 5f, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                LunaScreenMsg.PostScreenMessage($"{missingPartsCount} missing part(s) from Common.dll printed to log ({PartLoader.LoadedPartsList.Count} total). " +
                    $"{missingResourcesCount} missing resources from Common.dll printed to log ({PartResourceLibrary.Instance.resourceDefinitions.Count} total)", 
                    5f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        public IEnumerable<string> GetBannedPartsFromPartNames(IEnumerable<string> partNames)
        {
            var bannedParts = partNames.Where(p => !ModControlData.AllowedParts.Contains(p)).ToList();
            return bannedParts.Distinct();
        }

        public IEnumerable<string> GetBannedResourcesFromResourceNames(IEnumerable<string> resourceNames)
        {
            var bannedResources = resourceNames.Where(r => !ModControlData.AllowedResources.Contains(r)).ToList();
            return bannedResources.Distinct();
        }

        public List<string> GetInstalledExpansions()
        {
            var expansionsInfo = ExpansionsInfo?.GetValue(ExpansionsLoader.Instance);
            if (expansionsInfo != null)
            {
                var type = expansionsInfo.GetType();
                if (type.GetProperty("Keys")?.GetValue(expansionsInfo, null) is ICollection<string> keys)
                {
                    return keys.ToList();
                }
            }

            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Return the *.dll files that you have in the GameData folder and it's subdirectories except the ones from LMP and Squad
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<string> GetModFiles()
        {
            var gameDataDir = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData");

            foreach (var modDirectory in Directory.GetDirectories(gameDataDir))
            {
                var relPathFolder = modDirectory.Substring(modDirectory.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).Replace("\\", "/");
                if (relPathFolder.StartsWith("squad", StringComparison.OrdinalIgnoreCase) || relPathFolder.StartsWith("lunamultiplayer", StringComparison.OrdinalIgnoreCase))
                    continue;

                var filesInModFolder = Directory.GetFiles(modDirectory, "*.dll", SearchOption.AllDirectories);
                foreach (var file in filesInModFolder)
                {
                    yield return file;
                }
            }
        }

        private static string GetRelativePath(string file)
        {
            return file.ToLowerInvariant()
                .Substring(file.ToLowerInvariant().IndexOf("gamedata", StringComparison.Ordinal) + 9)
                .Replace('\\', '/');
        }

        #endregion
    }
}

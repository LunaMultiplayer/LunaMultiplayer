using Expansions;
using LunaClient.Localization;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.ModFile.Structure;
using LunaCommon.Xml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LunaClient.Systems.Mod
{
    public class ModSystem : Base.System<ModSystem>
    {
        #region Fields & properties

        public Dictionary<string, string> DllList { get; } = new Dictionary<string, string>();
        public bool ModControl { get; set; } = true;

        public ModControlStructure ModControlData { get; set; }
        public List<string> AllowedParts { get; set; } = new List<string>();

        public List<string> MissingExpansions { get; } = new List<string>();
        public List<ForbiddenDllFile> ForbiddenFilesFound { get; } = new List<ForbiddenDllFile>();
        public List<ForbiddenPart> ForbiddenPartsFound { get; } = new List<ForbiddenPart>();
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
            MissingExpansions.Clear();
            ForbiddenFilesFound.Clear();
            ForbiddenPartsFound.Clear();
            NonListedFilesFound.Clear();
            MandatoryFilesNotFound.Clear();
            MandatoryFilesDifferentSha.Clear();
            MandatoryPartsNotFound.Clear();

            ModControlData = null;
        }

        public void BuildDllFileList()
        {
            DllList.Clear();
            var checkList = Directory.GetFiles(CommonUtil.CombinePaths(MainSystem.KspPath, "GameData"), "*",
                SearchOption.AllDirectories);

            foreach (var checkFile in checkList.Where(f => f.ToLower().EndsWith(".dll")))
            {
                //We want the relative path to check against, example: LunaMultiplayer/Plugins/LunaMultiplayer.dll
                //Strip off everything from GameData
                //Replace windows backslashes with mac/linux forward slashes.
                //Make it lowercase so we don't worry about case sensitivity.
                var relativeFilePath = checkFile.ToLowerInvariant()
                    .Substring(checkFile.ToLowerInvariant().IndexOf("gamedata", StringComparison.Ordinal) + 9)
                    .Replace('\\', '/');

                var fileHash = Common.CalculateSha256FileHash(checkFile);
                DllList.Add(relativeFilePath, fileHash);

                //LunaLog.Log($"[LMP]: Hashed file: {relativeFilePath}, hash: {fileHash}");
            }
        }

        public void GenerateModControlFile(bool appendSha)
        {
            var modFile = LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(LunaCommon.Properties.Resources.LMPModControl);

            modFile.RequiredExpansions = GetInstalledExpansions();

            var extraParts = PartLoader.LoadedPartsList.Where(p => !modFile.AllowedParts.Contains(p.name)).Select(p => p.name);
            modFile.AllowedParts.AddRange(extraParts);

            var gameDataDir = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData");

            foreach (var modDirectory in Directory.GetDirectories(gameDataDir))
            {
                var relPathFolder = modDirectory.Substring(modDirectory.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).Replace("\\", "/");
                if (relPathFolder.StartsWith("squad", StringComparison.OrdinalIgnoreCase) || relPathFolder.StartsWith("lunamultiplayer", StringComparison.OrdinalIgnoreCase))
                    continue;

                var filesInModFolder = Directory.GetFiles(modDirectory, "*.dll", SearchOption.AllDirectories);
                foreach (var file in filesInModFolder)
                {
                    var relativeFilePath = file.Substring(file.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).Replace("\\", "/");
                    modFile.MandatoryPlugins.Add(new MandatoryDllFile
                    {
                        FilePath = relativeFilePath,
                        Sha = appendSha ? Common.CalculateSha256FileHash(file) : string.Empty,
                        Text = $"{Path.GetFileNameWithoutExtension(file)}. Version: {FileVersionInfo.GetVersionInfo(file).FileVersion}"
                    });
                }
            }

            LunaXmlSerializer.WriteToXmlFile(modFile, CommonUtil.CombinePaths(MainSystem.KspPath, "LMPModControl.xml"));
            ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.ModFileGenerated, 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public void CheckCommonStockParts()
        {
            var missingPartsCount = 0;
            LunaLog.Log("[LMP]: Missing parts start");
            var modFile = LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(LunaCommon.Properties.Resources.LMPModControl);
            var missingParts = PartLoader.LoadedPartsList.Where(p => !modFile.AllowedParts.Contains(p.name));

            foreach (var part in missingParts)
            {
                missingPartsCount++;
                LunaLog.Log($"[LMP]: Missing '{part.name}'");
            }

            LunaLog.Log("[LMP]: Missing parts end");

            ScreenMessages.PostScreenMessage(
                missingPartsCount > 0
                    ? $"{missingPartsCount} missing part(s) from Common.dll printed to debug log ({PartLoader.LoadedPartsList.Count} total)"
                    : $"No missing parts out of from Common.dll ({PartLoader.LoadedPartsList.Count} total)",
                5f, ScreenMessageStyle.UPPER_CENTER);
        }

        public IEnumerable<string> GetBannedPartsFromVessel(ProtoVessel protoVessel)
        {
            //Iterate in the PROTOVESSEL parts and not in the vessel parts as the part names in the vessel are fucked
            var bannedParts = new List<string>();
            foreach (var protoPart in protoVessel.protoPartSnapshots)
            {
                if (!ModControlData.AllowedParts.Contains(protoPart.partName))
                    bannedParts.Add(protoPart.partName);
            }

            return bannedParts.Distinct();
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
    }
}
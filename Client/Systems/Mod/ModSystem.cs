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

namespace LunaClient.Systems.Mod
{
    public class ModSystem : Base.System<ModSystem>
    {
        #region Fields & properties

        public bool ModControl { get; set; } = true;
        public Dictionary<string, string> DllList { get; } = new Dictionary<string, string>();
        public List<string> AllowedParts { get; set; } = new List<string>();
        public ModControlStructure ModControlData { get; set; }
        public List<ForbiddenDllFile> ForbiddenFilesFound { get; } = new List<ForbiddenDllFile>();
        public List<string> NonListedFilesFound { get; } = new List<string>();
        public List<MandatoryDllFile> MandatoryFilesNotFound { get; } = new List<MandatoryDllFile>();
        public List<MandatoryDllFile> MandatoryFilesDifferentSha { get; } = new List<MandatoryDllFile>();
        public ModFileHandler ModFileHandler { get; } = new ModFileHandler();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ModSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ModControl = true;
            ForbiddenFilesFound.Clear();
            MandatoryFilesNotFound.Clear();
            DllList.Clear();
            AllowedParts.Clear();
            ModControlData = null;
        }

        public override int ExecutionOrder => int.MinValue + 1;

        #endregion

        #region Public methods

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

        public IEnumerable<string> GetBannedPartsFromVessel(Vessel vessel)
        {
            var bannedParts = new List<string>();
            foreach (var part in vessel.parts)
            {
                if (!ModControlData.AllowedParts.Contains(part.name))
                    bannedParts.Add(part.name);
            }

            return bannedParts.Distinct();
        }

        #endregion
    }
}
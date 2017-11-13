using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaClient.Systems.Mod
{
    public class ModSystem : Base.System
    {
        #region Fields & properties

        public ModControlMode ModControl { get; set; } = ModControlMode.EnabledStopInvalidPartSync;
        public string FailText { get; set; }

        public Dictionary<string, string> DllList { get; } = new Dictionary<string, string>();
        public List<string> AllowedParts { get; set; } = new List<string>();
        public string LastModFileData { get; set; } = "";

        #endregion

        #region Base overrides

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ModControl = ModControlMode.EnabledStopInvalidPartSync;
            FailText = "";
            DllList.Clear();
            AllowedParts.Clear();
            LastModFileData = "";
        }

        #endregion

        #region Public methods

        public void BuildDllFileList()
        {
            DllList.Clear();
            var checkList = Directory.GetFiles(CommonUtil.CombinePaths(Client.KspPath, "GameData"), "*",
                SearchOption.AllDirectories);

            foreach (var checkFile in checkList.Where(f => f.ToLower().EndsWith(".dll")))
            {
                //We want the relative path to check against, example: LunaMultiPlayer/Plugins/LunaMultiPlayer.dll
                //Strip off everything from GameData
                //Replace windows backslashes with mac/linux forward slashes.
                //Make it lowercase so we don't worry about case sensitivity.
                var relativeFilePath = checkFile.ToLowerInvariant()
                    .Substring(checkFile.ToLowerInvariant().IndexOf("gamedata", StringComparison.Ordinal) + 9)
                    .Replace('\\', '/');

                var fileHash = Common.CalculateSha256Hash(checkFile);
                DllList.Add(relativeFilePath, fileHash);

                //LunaLog.Log($"[LMP]: Hashed file: {relativeFilePath}, hash: {fileHash}");
            }
        }

        public void GenerateModControlFile(bool whitelistMode)
        {
            var requiredFiles = new List<string>();
            var optionalFiles = new List<string>();
            var partsList = Common.GetStockParts();

            var gameDataDir = CommonUtil.CombinePaths(Client.KspPath, "GameData");

            var relativeModDirectories = Directory.GetDirectories(gameDataDir)
                .Select(d => d.Substring(d.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).ToLower())
                .Where(d => !d.StartsWith("squad", StringComparison.OrdinalIgnoreCase)
                            && !d.StartsWith("nasamission", StringComparison.OrdinalIgnoreCase)
                            && !d.StartsWith("lunamultiplayer", StringComparison.OrdinalIgnoreCase));

            //Add top level dll's to required (It's usually things like modulemanager)
            requiredFiles.AddRange(
                Directory.GetFiles(gameDataDir)
                    .Where(f => Path.GetExtension(f)?.ToLower() == ".dll")
                    .Select(Path.GetFileName));

            foreach (var modDirectory in relativeModDirectories)
            {
                var modIsRequired = false;

                var filesInModFolder = Directory.GetFiles(CommonUtil.CombinePaths(gameDataDir, modDirectory), "*",
                    SearchOption.AllDirectories);
                var modDllFiles = new List<string>();
                var modPartCfgFiles = new List<string>();

                foreach (var file in filesInModFolder)
                {
                    var relativeFileName = file.Substring(file.ToLower().IndexOf("gamedata", StringComparison.Ordinal) +
                                                          9)
                        .Replace(@"\", "/");

                    switch (Path.GetExtension(file).ToLower())
                    {
                        case ".dll":
                            modDllFiles.Add(relativeFileName);
                            break;
                        case ".cfg":
                            if (Path.GetExtension(file).ToLower() == ".cfg")
                            {
                                var fileIsPartFile = false;

                                var cn = ConfigNode.Load(file);
                                if (cn == null) continue;
                                foreach (var partName in cn.GetNodes("PART").Select(p => p.GetValue("Name")))
                                {
                                    LunaLog.Log($"[LMP]: Part detected in {relativeFileName} , Name: {partName}");
                                    modIsRequired = true;
                                    fileIsPartFile = true;
                                    partsList.Add(partName.Replace('_', '.'));
                                }

                                if (fileIsPartFile)
                                    modPartCfgFiles.Add(relativeFileName);
                            }
                            break;
                    }
                }

                if (modIsRequired)
                {
                    //If the mod as a plugin, just require that. It's clear enough.
                    //If the mod does *not* have a plugin (Scoop-o-matic is an example), add the part files to required instead.
                    requiredFiles.AddRange(modDllFiles.Count > 0 ? modDllFiles : modPartCfgFiles);
                }
                else
                {
                    if (whitelistMode)
                        optionalFiles.AddRange(modDllFiles);
                }
            }

            var modFileData = Common.GenerateModFileStringData(requiredFiles.ToArray(), optionalFiles.ToArray(),
                whitelistMode,
                new string[0], partsList.ToArray());

            using (var sw = new StreamWriter(CommonUtil.CombinePaths(Client.KspPath, "LMPModControl.txt"), false))
            {
                sw.Write(modFileData);
            }

            ScreenMessages.PostScreenMessage("LMPModFile.txt file generated in your KSP folder", 5f,
                ScreenMessageStyle.UPPER_CENTER);
        }

        public void CheckCommonStockParts()
        {
            var missingPartsCount = 0;
            LunaLog.Log("[LMP]: Missing parts start");
            var missingParts = PartLoader.LoadedPartsList.Where(p => !Common.GetStockParts().Contains(p.name));

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

        #endregion
    }
}
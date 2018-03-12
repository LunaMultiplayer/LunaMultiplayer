using LunaClient.Localization;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.ModFile.Structure;
using LunaCommon.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaClient.Systems.Mod
{
    public class ModSystem : Base.System<ModSystem>
    {
        #region Fields & properties

        public bool ModControl { get; set; } = true;
        public string FailText { get; set; }

        public Dictionary<string, string> DllList { get; } = new Dictionary<string, string>();
        public List<string> AllowedParts { get; set; } = new List<string>();
        public string LastModFileData { get; set; } = "";

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(ModSystem);

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ModControl = true;
            FailText = "";
            DllList.Clear();
            AllowedParts.Clear();
            LastModFileData = "";
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

        public void GenerateModControlFile()
        {
            var modFile = LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(LunaCommon.Properties.Resources.LMPModControl);

            var gameDataDir = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData");

            var relativeModDirectories = Directory.GetDirectories(gameDataDir)
                .Select(d => d.Substring(d.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).ToLower())
                .Where(d => !d.StartsWith("squad", StringComparison.OrdinalIgnoreCase)
                            && !d.StartsWith("lunamultiplayer", StringComparison.OrdinalIgnoreCase));

            //Add top level dll's
            modFile.MandatoryPlugins.AddRange(
                Directory.GetFiles(gameDataDir)
                    .Where(f => Path.GetExtension(f).ToLower() == ".dll")
                    .Select(f => new MandatoryDllFile
                    {
                        FilePath = Path.GetFileName(f)
                    }));

            modFile.ForbiddenPlugins.Add("exampleforbiddenpluginpath1/exampleforbiddenFile1.dll");
            modFile.ForbiddenPlugins.Add("exampleforbiddenpluginpath2/exampleforbiddenFile2.dll");

            foreach (var modDirectory in relativeModDirectories)
            {
                var filesInModFolder = Directory.GetFiles(CommonUtil.CombinePaths(gameDataDir, modDirectory), "*.dll", SearchOption.AllDirectories);
                foreach (var file in filesInModFolder)
                {
                    var relativeFileName = file.Substring(file.ToLower().IndexOf("gamedata", StringComparison.Ordinal) + 9).Replace(@"\", "/");
                    modFile.MandatoryPlugins.Add(new MandatoryDllFile
                    {
                        FilePath = relativeFileName
                    });
                }
            }

            LunaXmlSerializer.WriteToXmlFile(modFile, CommonUtil.CombinePaths(MainSystem.KspPath, "LMPModControl.xml"));
            ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.ModFileGenerated, 5f, ScreenMessageStyle.UPPER_CENTER);
        }

        #endregion
    }
}
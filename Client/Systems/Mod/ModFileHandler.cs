using LunaClient.Base;
using LunaClient.Utilities;
using LunaClient.Windows.Mod;
using LunaCommon.ModFile.Structure;
using LunaCommon.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.Mod
{
    public class ModFileHandler: SubSystem<ModSystem>
    {
        private static readonly StringBuilder Sb = new StringBuilder();

        public bool ParseModFile(ModControlStructure modFileData)
        {
            if (!ModSystem.Singleton.ModControl)
                return true;

            System.ModControlData = modFileData;
            Sb.Length = 0;

            SaveCurrentModConfigurationFile();

            SetAllPathsToLowercase(modFileData);
            if (!CheckFilesAndExpansions(modFileData))
            {
                LunaLog.LogError("[LMP]: Mod check failed!");
                LunaLog.LogError(Sb.ToString());
                ModWindow.Singleton.Display = true;
                return false;
            }

            System.AllowedParts = modFileData.AllowedParts;
            LunaLog.Log("[LMP]: Mod check passed!");
            return true;
        }

        #region Check mod files


        private static void SetAllPathsToLowercase(ModControlStructure modFileInfo)
        {
            modFileInfo.MandatoryPlugins.ForEach(m => m.FilePath = m.FilePath.ToLower());
            modFileInfo.ForbiddenPlugins.ForEach(m => m.FilePath = m.FilePath.ToLower());
        }

        private static void SaveCurrentModConfigurationFile()
        {
            var tempModFilePath = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Plugins", "Data", "LMPModControl.xml");
            LunaXmlSerializer.WriteToXmlFile(System.ModControlData, tempModFilePath);
        }

        private static bool CheckFilesAndExpansions(ModControlStructure modInfo)
        {
            var checkOk = true;

            var currentExpansions = System.GetInstalledExpansions();
            var missingExpansions = modInfo.RequiredExpansions.Except(currentExpansions).ToArray();
            if (missingExpansions.Any())
            {
                Sb.AppendLine($"Missing {string.Join(", ", missingExpansions)} expansion/s!");
                System.MissingExpansions.AddRange(missingExpansions);
                checkOk = false;
            }

            foreach (var file in System.DllList)
            {
                checkOk &= CheckExistingFile(modInfo, file);
            }

            foreach (var requiredEntry in modInfo.MandatoryPlugins)
            {
                checkOk &= CheckMandatoryFile(requiredEntry);
            }

            return checkOk;
        }

        private static bool CheckExistingFile(ModControlStructure modInfo, KeyValuePair<string, string> file)
        {
            var forbiddenItem = modInfo.ForbiddenPlugins.FirstOrDefault(f => f.FilePath == file.Key);
            if (forbiddenItem != null)
            {
                Sb.AppendLine($"Banned file {file.Key} exists on client!");
                System.ForbiddenFilesFound.Add(forbiddenItem);
                return false;
            }

            if (!modInfo.AllowNonListedPlugins && modInfo.MandatoryPlugins.All(f => f.FilePath != file.Key))
            {
                Sb.AppendLine($"Server does not allow external plugins and banned file {file.Key} exists on client!");
                System.NonListedFilesFound.Add(file.Key);
                return false;
            }

            return true;
        }

        private static bool CheckMandatoryFile(MandatoryDllFile item)
        {
            var fileExists = System.DllList.ContainsKey(item.FilePath);
            if (!fileExists)
            {
                Sb.AppendLine($"Required file {item.FilePath} is missing!");
                System.MandatoryFilesNotFound.Add(item);

                return false;
            }
            if (!string.IsNullOrEmpty(item.Sha) && System.DllList[item.FilePath] != item.Sha)
            {
                Sb.AppendLine($"Required file {item.FilePath} does not match hash {item.Sha}!");
                System.MandatoryFilesDifferentSha.Add(item);

                return false;
            }

            return true;
        }

        #endregion
    }
}
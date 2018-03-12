using LunaClient.Utilities;
using LunaClient.Windows.Mod;
using LunaCommon.ModFile;
using LunaCommon.ModFile.Structure;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.Mod
{
    public class ModFileHandler
    {
        private static readonly StringBuilder Sb = new StringBuilder();

        public static bool ParseModFile(string modFileData)
        {
            if (!ModSystem.Singleton.ModControl)
                return true;

            Sb.Length = 0;
            ModSystem.Singleton.LastModFileData = modFileData; //Save mod file so we can recheck it.

            SaveCurrentModConfigurationFile();

            var modFileInfo = ModFileParser.ReadModFileFromString(modFileData);
            if (!CheckFiles(modFileInfo))
            {
                LunaLog.LogError("[LMP]: Mod check failed!");
                LunaLog.LogError(Sb.ToString());
                ModSystem.Singleton.FailText = Sb.ToString();
                ModWindow.Singleton.Display = true;
                return false;
            }

            ModSystem.Singleton.AllowedParts = modFileInfo.AllowedParts;
            LunaLog.Log("[LMP]: Mod check passed!");
            return true;
        }

        private static void SaveCurrentModConfigurationFile()
        {
            var tempModFilePath = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Plugins", "Data", "LMPModControl.xml");
            File.WriteAllText(tempModFilePath, ModSystem.Singleton.LastModFileData);
        }

        #region Check mod files

        private static bool CheckFiles(ModControlStructure modInfo)
        {
            var checkOk = true;

            foreach (var file in ModSystem.Singleton.DllList)
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
            if (modInfo.ForbiddenPlugins.Any(f => f == file.Key))
            {
                Sb.AppendLine($"Banned file {file.Key} exists on client!");
                return false;
            }

            if (!modInfo.AllowNonListedPlugins && modInfo.MandatoryPlugins.All(f => f.FilePath != file.Key))
            {
                Sb.AppendLine($"Banned file {file.Key} exists on client!");
                return false;
            }

            return true;
        }

        private static bool CheckMandatoryFile(MandatoryDllFile item)
        {
            var fileExists = ModSystem.Singleton.DllList.ContainsKey(item.FilePath);
            if (!fileExists)
            {
                Sb.AppendLine($"Required file {item.FilePath} is missing!");
                return false;
            }
            if (!string.IsNullOrEmpty(item.Sha) && ModSystem.Singleton.DllList[item.FilePath] != item.Sha)
            {
                Sb.AppendLine($"Required file {item.FilePath} does not match hash {item.Sha}!");
                return false;
            }

            return true;
        }

        #endregion
    }
}
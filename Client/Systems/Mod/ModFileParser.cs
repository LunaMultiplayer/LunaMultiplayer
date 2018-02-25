using LunaClient.Utilities;
using LunaClient.Windows.Mod;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.ModFile;
using System;
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
            if (ModSystem.Singleton.ModControl == ModControlMode.Disabled)
                return true;

            Sb.Length = 0;
            ModSystem.Singleton.LastModFileData = modFileData; //Save mod file so we can recheck it.

            SaveCurrentModConfigurationFile();

            var modFileInfo = ModFileParser.ReadModFile(modFileData);
            if (!CheckFiles(modFileInfo))
            {
                LunaLog.LogError("[LMP]: Mod check failed!");
                LunaLog.LogError(Sb.ToString());
                ModSystem.Singleton.FailText = Sb.ToString();
                ModWindow.Singleton.Display = true;
                return false;
            }

            ModSystem.Singleton.AllowedParts = modFileInfo.PartList;
            LunaLog.Log("[LMP]: Mod check passed!");
            return true;
        }

        private static void SaveCurrentModConfigurationFile()
        {
            var tempModFilePath = CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer", "Plugins", "Data", "LMPModControl.txt");
            using (var sw = new StreamWriter(tempModFilePath, false))
            {
                sw.WriteLine("#This file is downloaded from the server during connection. It is saved here for convenience.");
                sw.WriteLine(ModSystem.Singleton.LastModFileData);
            }
        }
        
        #region Check mod files

        private static bool CheckFiles(ModInformation modInfo)
        {
            var checkOk = true;

            var gameFilePaths = Directory.GetFiles(CommonUtil.CombinePaths(Client.KspPath, "GameData"), "*", SearchOption.AllDirectories);
            var gameFileRelativePaths = gameFilePaths
                .Select( filePath =>filePath.Substring(filePath.ToLowerInvariant().IndexOf("gamedata", StringComparison.Ordinal) + 9)
                .Replace('\\', '/')).ToArray();
            
            foreach (var requiredEntry in modInfo.RequiredFiles)
            {
                checkOk &= CheckFile(gameFileRelativePaths, requiredEntry, true);
            }
            
            foreach (var optionalEntry in modInfo.OptionalFiles)
            {
                checkOk &= CheckFile(gameFileRelativePaths, optionalEntry, false);
            }

            if (modInfo.WhiteListFiles.Any()) //Check Resource whitelist
            {
                checkOk &= CheckWhiteList(modInfo);
            }

            if (modInfo.BlackListFiles.Any()) //Check Resource blacklist
            {
                checkOk &= CheckBlackList(modInfo);
            }

            return checkOk;
        }

        private static bool CheckBlackList(ModInformation modInfo)
        {
            var invalidMods = modInfo.BlackListFiles.Where(f => ModSystem.Singleton.DllList.ContainsKey(f.ModFilename)).ToArray();
            foreach (var blacklistEntry in invalidMods)
            {
                Sb.AppendLine($"Banned resource {blacklistEntry} exists on client!");
            }

            return !invalidMods.Any();
        }

        private static bool CheckWhiteList(ModInformation modInfo)
        {
            //Allow LMP files, Ignore squad plugins, Check required (Required implies whitelist), Check optional (Optional implies whitelist)
            var invalidMods = ModSystem.Singleton.DllList.Where(
                f => !AutoAllowedMods.AllowedMods.Contains(f.Key) &&
                     !f.Key.StartsWith("squad/plugins") &&
                     modInfo.RequiredFiles.All(m => m.ModFilename != f.Key) &&
                     modInfo.OptionalFiles.All(m => m.ModFilename != f.Key) &&
                     modInfo.WhiteListFiles.All(m => m.ModFilename != f.Key)).ToArray();

            foreach (var dllResource in invalidMods)
            {
                Sb.AppendLine($"Non-whitelisted resource {dllResource.Key} exists on client!");
            }

            return !invalidMods.Any();
        }

        private static bool CheckFile(IEnumerable<string> gameFileRelativePaths, ModItem item, bool required)
        {
            if (item.ModFilename.EndsWith("dll"))
            {
                var fileExists = ModSystem.Singleton.DllList.ContainsKey(item.ModFilename);
                if (!fileExists && required)
                {
                    Sb.AppendLine($"Required file {item.ModFilename} is missing!");
                    return false;
                }
                if (fileExists && !string.IsNullOrEmpty(item.Sha) && ModSystem.Singleton.DllList[item.ModFilename] != item.Sha)
                {
                    Sb.AppendLine($"Required file {item.ModFilename} does not match hash {item.Sha}!");
                    return false;
                }
            }
            else
            {
                var filePath = gameFileRelativePaths.FirstOrDefault(f => f.Equals(item.ModFilename, StringComparison.OrdinalIgnoreCase));
                if (string.IsNullOrEmpty(filePath) && required) //Only show error if the file is required
                {
                    Sb.AppendLine($"Required file {item.ModFilename} is missing!");
                    return false;
                }
                if (!string.IsNullOrEmpty(filePath) && !string.IsNullOrEmpty(item.Sha))
                {
                    var fullFilePath = CommonUtil.CombinePaths(Client.KspPath, "GameData", filePath);
                    if (Common.CalculateSha256Hash(fullFilePath) != item.Sha)
                    {
                        Sb.AppendLine($"File {item.ModFilename} does not match hash {item.Sha}!");
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
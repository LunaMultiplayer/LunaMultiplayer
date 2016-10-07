using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LunaClient.Utilities;
using LunaClient.Windows.Mod;
using LunaCommon;
using LunaCommon.Enums;

namespace LunaClient.Systems.Mod
{
    public class ModFileParser
    {
        public static bool ParseModFile(string modFileData)
        {
            if (ModSystem.Singleton.ModControl == ModControlMode.DISABLED) return true;

            ModSystem.Singleton.LastModFileData = modFileData; //Save mod file so we can recheck it.

            StringBuilder = new StringBuilder();
            ParseRequired.Clear();
            ParseOptional.Clear();
            WhiteList.Clear();
            BlackList.Clear();
            PartsList.Clear();

            SaveCurrentModConfigurationFile();

            ReadModConfigurationFile(modFileData);

            CheckFiles();

            if (!ModCheckOk)
            {
                ModSystem.Singleton.FailText = StringBuilder.ToString();
                ModWindow.Singleton.Display = true;
                return false;
            }

            ModSystem.Singleton.AllowedParts = PartsList;
            LunaLog.Debug("Mod check passed!");
            return true;
        }

        private static void SaveCurrentModConfigurationFile()
        {
            var tempModFilePath = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", "LunaMultiPlayer",
                            "Plugins", "Data", "LMPModControl.txt");

            using (var sw = new StreamWriter(tempModFilePath))
            {
                sw.WriteLine("#This file is downloaded from the server during connection. It is saved here for convenience.");
                sw.WriteLine(ModSystem.Singleton.LastModFileData);
            }
        }

        #region Fields

        private static bool ModCheckOk { get; set; } = true;
        private static StringBuilder StringBuilder { get; set; } = new StringBuilder();
        private static Dictionary<string, string> ParseRequired { get; } = new Dictionary<string, string>();
        private static Dictionary<string, string> ParseOptional { get; } = new Dictionary<string, string>();
        private static Dictionary<string, string> WhiteList { get; } = new Dictionary<string, string>();
        private static Dictionary<string, string> BlackList { get; } = new Dictionary<string, string>();
        private static List<string> PartsList { get; } = new List<string>();

        #endregion

        #region Read mod file

        private static void ReadModConfigurationFile(string modFileData)
        {
            var readMode = "";
            using (var sr = new StringReader(modFileData))
            {
                string trimmedLine;
                while ((trimmedLine = sr.ReadLine()?.Trim().ToLowerInvariant().Replace('\\', '/')) != null)
                {
                    if (trimmedLine.StartsWith("#") || string.IsNullOrEmpty(trimmedLine))
                        continue; //Skip comments or empty lines.

                    if (trimmedLine.StartsWith("!"))
                        readMode = GetReadMode(trimmedLine);
                    else
                    {
                        switch (readMode)
                        {
                            case "required-files":
                                FillDictionaryWithFiles(trimmedLine, ParseRequired);
                                break;
                            case "optional-files":
                                FillDictionaryWithFiles(trimmedLine, ParseOptional);
                                break;
                            case "resource-whitelist":
                                FillDictionaryWithFiles(trimmedLine, WhiteList);
                                break;
                            case "resource-blacklist":
                                FillDictionaryWithFiles(trimmedLine, BlackList);
                                break;
                            case "partslist":
                                if (!PartsList.Contains(trimmedLine))
                                    PartsList.Add(trimmedLine);
                                break;
                        }
                    }
                }
            }
        }

        private static void FillDictionaryWithFiles(string fileLine, Dictionary<string, string> dictionary)
        {
            if (fileLine.Contains("="))
            {
                var splitLine = fileLine.Split('=');
                if (!dictionary.ContainsKey(splitLine[0]))
                    dictionary.Add(splitLine[0], splitLine.Length == 2 ? splitLine[1].ToLowerInvariant() : "");
            }
            else
            {
                if (!dictionary.ContainsKey(fileLine))
                    dictionary.Add(fileLine, "");
            }
        }

        private static string GetReadMode(string trimmedLine)
        {
            //New section
            switch (trimmedLine.Substring(1))
            {
                case "required-files":
                case "optional-files":
                case "partslist":
                case "resource-blacklist":
                case "resource-whitelist":
                    return trimmedLine.Substring(1);
                default:
                    return trimmedLine;
            }
        }

        #endregion

        #region Check mod files

        private static void CheckFiles()
        {
            var gameFilePaths = Directory.GetFiles(CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData"), "*",
                SearchOption.AllDirectories);
            var gameFileRelativePaths =
                gameFilePaths.Select(
                    filePath =>
                        filePath.Substring(filePath.ToLowerInvariant().IndexOf("gamedata", StringComparison.Ordinal) + 9)
                            .Replace('\\', '/')).ToList();

            //Check Required
            foreach (var requiredEntry in ParseRequired)
            {
                if (!requiredEntry.Key.EndsWith("dll"))
                    CheckNonDllFile(gameFileRelativePaths, requiredEntry, true);
                else
                    CheckDllFile(requiredEntry, true);
            }

            //Check Optional
            foreach (var optionalEntry in ParseOptional)
            {
                if (!optionalEntry.Key.EndsWith("dll"))
                    CheckNonDllFile(gameFileRelativePaths, optionalEntry, false);
                else
                    CheckDllFile(optionalEntry, false);
            }

            if (WhiteList.Any() && !BlackList.Any()) //Check Resource whitelist
            {
                var autoAllowed = new List<string>
                {
                    "lunamultiplayer/plugins/lunaclient.dll",
                    "lunamultiplayer/plugins/lunacommon.dll",
                    "lunamultiplayer/plugins/fastmember.dll",
                    "lunamultiplayer/plugins/lidgren.network.dll",
                    "lunamultiplayer/plugins/mono.data.tds.dll",
                    "lunamultiplayer/plugins/system.data.dll",
                    "lunamultiplayer/plugins/system.threading.dll",
                    "lunamultiplayer/plugins/system.transactions.dll"
                };

                //Allow LMP files, Ignore squad plugins, Check required (Required implies whitelist), Check optional (Optional implies whitelist)
                //Check whitelist

                foreach (var dllResource in
                    ModSystem.Singleton.DllList.Where(
                        dllResource =>
                            !autoAllowed.Contains(dllResource.Key) && !dllResource.Key.StartsWith("squad/plugins") &&
                            !ParseRequired.ContainsKey(dllResource.Key) && !ParseOptional.ContainsKey(dllResource.Key) &&
                            !WhiteList.ContainsKey(dllResource.Key)))
                {
                    ModCheckOk = false;
                    LunaLog.Debug("Non-whitelisted resource " + dllResource.Key + " exists on client!");
                    StringBuilder.AppendLine("Non-whitelisted resource " + dllResource.Key + " exists on client!");
                }
            }

            if (!WhiteList.Any() && BlackList.Any()) //Check Resource blacklist
            {
                foreach (var blacklistEntry in
                    BlackList.Keys.Where(blacklistEntry => ModSystem.Singleton.DllList.ContainsKey(blacklistEntry)))
                {
                    ModCheckOk = false;
                    LunaLog.Debug("Banned resource " + blacklistEntry + " exists on client!");
                    StringBuilder.AppendLine("Banned resource " + blacklistEntry + " exists on client!");
                }
            }
        }

        private static void CheckDllFile(KeyValuePair<string, string> requiredEntry, bool required)
        {
            var fileExists = ModSystem.Singleton.DllList.ContainsKey(requiredEntry.Key);

            if (!fileExists && required)
            {
                ModCheckOk = false;
                LunaLog.Debug("Required file " + requiredEntry.Key + " is missing!");
                StringBuilder.AppendLine("Required file " + requiredEntry.Key + " is missing!");
                return;
            }

            if (fileExists && (requiredEntry.Value != "") &&
                (ModSystem.Singleton.DllList[requiredEntry.Key] != requiredEntry.Value))
            {
                ModCheckOk = false;
                LunaLog.Debug("Required file " + requiredEntry.Key + " does not match hash " + requiredEntry.Value + "!");
                StringBuilder.AppendLine("Required file " + requiredEntry.Key + " does not match hash " +
                                         requiredEntry.Value + "!");
            }
        }

        private static void CheckNonDllFile(List<string> gameFileRelativePaths,
            KeyValuePair<string, string> requiredEntry, bool required)
        {
            var filePath = gameFileRelativePaths.FirstOrDefault(
                f => f.Equals(requiredEntry.Key, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(filePath) && required) //Only show error if the file is required
            {
                ModCheckOk = false;
                LunaLog.Debug("Required file " + requiredEntry.Key + " is missing!");
                StringBuilder.AppendLine("Required file " + requiredEntry.Key + " is missing!");
                return;
            }

            //If the entry exists and has a SHA sum, we need to check it.
            if (!string.IsNullOrEmpty(filePath) && (requiredEntry.Value != ""))
                EvaluateShaSum(filePath, requiredEntry);
        }

        private static void EvaluateShaSum(string filePath, KeyValuePair<string, string> fileEntry)
        {
            var fullFilePath = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", filePath);
            if (!CheckFile(fullFilePath, fileEntry.Value))
            {
                ModCheckOk = false;
                LunaLog.Debug("File " + fileEntry.Key + " does not match hash " + fileEntry.Value + "!");
                StringBuilder.AppendLine("File " + fileEntry.Key + " does not match hash " + fileEntry.Value + "!");
            }
        }

        private static bool CheckFile(string relativeFileName, string referencefileHash)
        {
            var fullFileName = CommonUtil.CombinePaths(KSPUtil.ApplicationRootPath, "GameData", relativeFileName);
            var fileHash = Common.CalculateSha256Hash(fullFileName);
            return fileHash == referencefileHash;
        }

        #endregion
    }
}
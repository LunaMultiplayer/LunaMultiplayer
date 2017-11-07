using LunaClient.Utilities;
using LunaClient.Windows;
using LunaClient.Windows.Mod;
using LunaCommon;
using LunaCommon.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LunaClient.Systems.Mod
{
    public class ModFileParser
    {
        public static bool ParseModFile(string modFileData)
        {
            if (SystemsContainer.Get<ModSystem>().ModControl == ModControlMode.Disabled) return true;

            SystemsContainer.Get<ModSystem>().LastModFileData = modFileData; //Save mod file so we can recheck it.

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
                SystemsContainer.Get<ModSystem>().FailText = StringBuilder.ToString();
                WindowsContainer.Get<ModWindow>().Display = true;
                return false;
            }

            SystemsContainer.Get<ModSystem>().AllowedParts = PartsList;
            LunaLog.Log("[LMP]: Mod check passed!");
            return true;
        }

        private static void SaveCurrentModConfigurationFile()
        {
            var tempModFilePath = CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer",
                            "Plugins", "Data", "LMPModControl.txt");

            using (var sw = new StreamWriter(tempModFilePath))
            {
                sw.WriteLine("#This file is downloaded from the server during connection. It is saved here for convenience.");
                sw.WriteLine(SystemsContainer.Get<ModSystem>().LastModFileData);
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
            var gameFilePaths = Directory.GetFiles(CommonUtil.CombinePaths(Client.KspPath, "GameData"), "*",
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
                    "lunamultiplayer/plugins/fastmember.dll",
                    "lunamultiplayer/plugins/lidgren.network.dll",
                    "lunamultiplayer/plugins/lunaclient.dll",
                    "lunamultiplayer/plugins/lunacommon.dll",
                    "lunamultiplayer/plugins/mono.data.tds.dll",
                    "lunamultiplayer/plugins/system.data.dll",
                    "lunamultiplayer/plugins/system.threading.dll",
                    "lunamultiplayer/plugins/system.transactions.dll"
                };

                //Allow LMP files, Ignore squad plugins, Check required (Required implies whitelist), Check optional (Optional implies whitelist)
                //Check whitelist

                foreach (var dllResource in
                    SystemsContainer.Get<ModSystem>().DllList.Where(
                        dllResource =>
                            !autoAllowed.Contains(dllResource.Key) && !dllResource.Key.StartsWith("squad/plugins") &&
                            !ParseRequired.ContainsKey(dllResource.Key) && !ParseOptional.ContainsKey(dllResource.Key) &&
                            !WhiteList.ContainsKey(dllResource.Key)))
                {
                    ModCheckOk = false;
                    LunaLog.Log($"[LMP]: Non-whitelisted resource {dllResource.Key} exists on client!");
                    StringBuilder.AppendLine($"Non-whitelisted resource {dllResource.Key} exists on client!");
                }
            }

            if (!WhiteList.Any() && BlackList.Any()) //Check Resource blacklist
            {
                foreach (var blacklistEntry in
                    BlackList.Keys.Where(blacklistEntry => SystemsContainer.Get<ModSystem>().DllList.ContainsKey(blacklistEntry)))
                {
                    ModCheckOk = false;
                    LunaLog.Log($"[LMP]: Banned resource {blacklistEntry} exists on client!");
                    StringBuilder.AppendLine($"Banned resource {blacklistEntry} exists on client!");
                }
            }
        }

        private static void CheckDllFile(KeyValuePair<string, string> requiredEntry, bool required)
        {
            var fileExists = SystemsContainer.Get<ModSystem>().DllList.ContainsKey(requiredEntry.Key);

            if (!fileExists && required)
            {
                ModCheckOk = false;
                LunaLog.Log($"[LMP]: Required file {requiredEntry.Key} is missing!");
                StringBuilder.AppendLine($"Required file {requiredEntry.Key} is missing!");
                return;
            }

            if (fileExists && requiredEntry.Value != "" &&
                SystemsContainer.Get<ModSystem>().DllList[requiredEntry.Key] != requiredEntry.Value)
            {
                ModCheckOk = false;
                LunaLog.Log($"[LMP]: Required file {requiredEntry.Key} does not match hash {requiredEntry.Value}!");
                StringBuilder.AppendLine($"Required file {requiredEntry.Key} does not match hash {requiredEntry.Value}!");
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
                LunaLog.Log($"[LMP]: Required file {requiredEntry.Key} is missing!");
                StringBuilder.AppendLine($"Required file {requiredEntry.Key} is missing!");
                return;
            }

            //If the entry exists and has a SHA sum, we need to check it.
            if (!string.IsNullOrEmpty(filePath) && requiredEntry.Value != "")
                EvaluateShaSum(filePath, requiredEntry);
        }

        private static void EvaluateShaSum(string filePath, KeyValuePair<string, string> fileEntry)
        {
            var fullFilePath = CommonUtil.CombinePaths(Client.KspPath, "GameData", filePath);
            if (!CheckFile(fullFilePath, fileEntry.Value))
            {
                ModCheckOk = false;
                LunaLog.Log($"[LMP]: File {fileEntry.Key} does not match hash {fileEntry.Value}!");
                StringBuilder.AppendLine($"File {fileEntry.Key} does not match hash {fileEntry.Value}!");
            }
        }

        private static bool CheckFile(string relativeFileName, string referencefileHash)
        {
            var fullFileName = CommonUtil.CombinePaths(Client.KspPath, "GameData", relativeFileName);
            var fileHash = Common.CalculateSha256Hash(fullFileName);
            return fileHash == referencefileHash;
        }

        #endregion
    }
}
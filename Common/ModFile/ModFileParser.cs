using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaCommon.ModFile
{
    public class ModFileParser
    {
        private static readonly ModInformation ModInfo = new ModInformation();

        public static ModInformation ReadModFile(string modFileContent)
        {
            ModInfo.Clear();
            using (var sr = new StringReader(modFileContent))
            {
                string trimmedLine;
                var section = string.Empty;

                while ((trimmedLine = sr.ReadLine()?.Trim()?.ToLowerInvariant()?.Replace('\\', '/')) != null)
                {
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        continue; //Skip comments or empty lines.

                    if (trimmedLine.StartsWith("!"))
                        section = GetModFileSection(trimmedLine);
                    else
                    {
                        switch (section)
                        {
                            case "required-files":
                                FillListWithFiles(trimmedLine, ModInfo.RequiredFiles);
                                break;
                            case "optional-files":
                                FillListWithFiles(trimmedLine, ModInfo.OptionalFiles);
                                break;
                            case "resource-whitelist":
                                if (!ModInfo.BlackListFiles.Any())
                                    FillListWithFiles(trimmedLine, ModInfo.WhiteListFiles);
                                break;
                            case "resource-blacklist":
                                if (!ModInfo.WhiteListFiles.Any())
                                    FillListWithFiles(trimmedLine, ModInfo.BlackListFiles);
                                break;
                            case "partslist":
                                if (!ModInfo.PartList.Contains(trimmedLine))
                                    ModInfo.PartList.Add(trimmedLine);
                                break;
                        }
                    }
                }
            }

            return ModInfo;
        }

        private static string GetModFileSection(string textLine)
        {
            switch (textLine.Substring(1))
            {
                case "required-files":
                case "optional-files":
                case "partslist":
                case "resource-blacklist":
                case "resource-whitelist":
                    return textLine.Substring(1);
                default:
                    return textLine;
            }
        }

        private static void FillListWithFiles(string fileLine, List<ModItem> files)
        {
            if (fileLine.Contains("="))
            {
                var splitLine = fileLine.Split('=');
                if (files.All(f => f.ModFilename != splitLine[0]))
                    files.Add(new ModItem { ModFilename = splitLine[0], Sha = splitLine.Length == 2 ? splitLine[1].ToLowerInvariant() : string.Empty });
            }
            else
            {
                if (files.All(f => f.ModFilename != fileLine))
                    files.Add(new ModItem { ModFilename = fileLine });
            }
        }
    }
}

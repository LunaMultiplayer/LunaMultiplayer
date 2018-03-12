using LunaCommon.ModFile.Structure;
using LunaCommon.Properties;
using LunaCommon.Xml;
using System;
using System.IO;

namespace LunaCommon.ModFile
{
    public class ModFileParser
    {
        public static ModControlStructure ReadModFileFromPath(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, Resources.LMPModControl);
                }

                return LunaXmlSerializer.ReadXmlFromPath<ModControlStructure>(filePath);
            }
            catch (Exception)
            {
                return LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(Resources.LMPModControl);
            }
        }

        public static ModControlStructure ReadModFileFromString(string contents)
        {
            return LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(contents);
        }
    }
}

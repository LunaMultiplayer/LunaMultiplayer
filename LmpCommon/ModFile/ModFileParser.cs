using LmpCommon.ModFile.Structure;
using LmpCommon.Properties;
using LmpCommon.Xml;
using System;
using System.IO;

namespace LmpCommon.ModFile
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

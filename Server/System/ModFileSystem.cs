using LunaCommon.ModFile.Structure;
using LunaCommon.Xml;
using Server.Context;
using Server.Log;
using System;

namespace Server.System
{
    public class ModFileSystem
    {
        public static ModControlStructure ModControl { get; private set; }

        public static void GenerateNewModFile()
        {
            FileHandler.WriteToFile(ServerContext.ModFilePath, LunaCommon.Properties.Resources.LMPModControl);
        }

        public static void LoadModFile()
        {
            try
            {
                ModControl = LunaXmlSerializer.ReadXmlFromPath<ModControlStructure>(ServerContext.ModFilePath);
            }
            catch (Exception)
            {
                LunaLog.Error("Cannot read LMPModControl file. Will load the default one. Please regenerate it");
                ModControl = LunaXmlSerializer.ReadXmlFromString<ModControlStructure>(LunaCommon.Properties.Resources.LMPModControl);
            }
        }
    }
}
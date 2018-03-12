using LunaCommon;
using LunaCommon.ModFile.Structure;
using LunaCommon.Xml;
using Server.Context;

namespace Server.System
{
    public class ModFileSystem
    {
        public static ModControlStructure ModControl { get; } = LunaXmlSerializer.ReadXmlFromPath<ModControlStructure>(ServerContext.ModFilePath);

        public static void GenerateNewModFile()
        {
            FileHandler.WriteToFile(ServerContext.ModFilePath, LunaCommon.Properties.Resources.LMPModControl);
        }

        //Get mod file SHA
        public static string GetModControlSha()
        {
            return Common.CalculateSha256FileHash(ServerContext.ModFilePath);
        }
    }
}
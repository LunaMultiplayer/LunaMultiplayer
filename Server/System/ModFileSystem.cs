using LunaCommon;
using Server.Context;

namespace Server.System
{
    public class ModFileSystem
    {
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
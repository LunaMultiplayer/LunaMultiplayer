using LMP.Server.Context;
using LunaCommon;

namespace LMP.Server.System
{
    public class ModFileSystem
    {
        public static void GenerateNewModFile()
        {
            if (FileHandler.FileExists(ServerContext.ModFilePath))
                FileHandler.MoveFile(ServerContext.ModFilePath, $"{ServerContext.ModFilePath}.bak");
            var modFileData = Common.GenerateModFileStringData(new string[0],
                new string[0], false, new string[0], Common.GetStockParts().ToArray());

            FileHandler.WriteToFile(ServerContext.ModFilePath, modFileData);
        }

        //Get mod file SHA
        public static string GetModControlSha()
        {
            return Common.CalculateSha256Hash(ServerContext.ModFilePath);
        }
    }
}
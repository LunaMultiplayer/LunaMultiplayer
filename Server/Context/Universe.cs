using Server.Log;
using Server.System;
using System.IO;
using System.Linq;

namespace Server.Context
{
    public class Universe
    {
        // Check universe folder size
        public static long GetUniverseSize()
        {
            var kerbals = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Kerbals"));
            var vessels = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"));

            var directorySize = kerbals.Select(kerbal => new FileInfo(kerbal)).Select(kInfo => kInfo.Length).Sum();
            directorySize += vessels.Select(vessel => new FileInfo(vessel)).Select(vInfo => vInfo.Length).Sum();

            return directorySize;
        }

        public static void RemoveFromUniverse(string path)
        {
            FileHandler.FileDelete(path);
        }

        //Create universe directories
        public static void CheckUniverse()
        {
            LunaLog.Debug("Loading universe... ");

            if (!FileHandler.FileExists(ServerContext.ModFilePath))
                ModFileSystem.GenerateNewModFile();
            if (!FileHandler.FolderExists(ServerContext.UniverseDirectory))
                FileHandler.FolderCreate(ServerContext.UniverseDirectory);

            if (!FileHandler.FolderExists(CraftLibrarySystem.CraftPath))
                FileHandler.FolderCreate(CraftLibrarySystem.CraftPath);
            if (!FileHandler.FolderExists(FlagSystem.FlagPath))
                FileHandler.FolderCreate(FlagSystem.FlagPath);
            if (!FileHandler.FolderExists(GroupSystem.GroupsPath))
                FileHandler.FolderCreate(GroupSystem.GroupsPath);
            if (!FileHandler.FolderExists(ScreenshotSystem.ScreenshotPath))
                FileHandler.FolderCreate(ScreenshotSystem.ScreenshotPath);
            if (!FileHandler.FolderExists(KerbalSystem.KerbalsPath))
            {
                FileHandler.FolderCreate(KerbalSystem.KerbalsPath);
                KerbalSystem.GenerateDefaultKerbals();
            }
            if (!FileHandler.FolderExists(ScenarioSystem.ScenarioPath))
                FileHandler.FolderCreate(ScenarioSystem.ScenarioPath);
            if (!FileHandler.FolderExists(VesselStoreSystem.VesselsPath))
                FileHandler.FolderCreate(VesselStoreSystem.VesselsPath);
        }
    }
}

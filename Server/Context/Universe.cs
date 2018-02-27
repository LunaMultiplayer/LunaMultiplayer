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
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Crafts")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Crafts"));
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Flags")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Flags"));
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Groups")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Groups"));
            if (!FileHandler.FolderExists(KerbalSystem.KerbalsPath))
            {
                FileHandler.FolderCreate(KerbalSystem.KerbalsPath);
                KerbalSystem.GenerateDefaultKerbals();
            }
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Players")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Players"));
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Relay")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Relay"));
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Scenarios")))
            {
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Scenarios"));
                ScenarioSystem.GenerateDefaultScenarios();
            }
            if (!FileHandler.FolderExists(Path.Combine(ServerContext.UniverseDirectory, "Vessels")))
                FileHandler.FolderCreate(Path.Combine(ServerContext.UniverseDirectory, "Vessels"));
        }
    }
}
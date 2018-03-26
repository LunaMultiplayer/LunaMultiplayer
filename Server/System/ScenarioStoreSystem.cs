using LunaCommon.Xml;
using Server.Context;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
// ReSharper disable InconsistentlySynchronizedField

namespace Server.System
{
    /// <summary>
    /// Here we keep a copy of all the scnarios modules in XML format and we also save them to files at a specified rate
    /// </summary>
    public class ScenarioStoreSystem
    {
        public static string ScenariosFolder = Path.Combine(ServerContext.UniverseDirectory, "Scenarios");

        public static ConcurrentDictionary<string, string> CurrentScenariosInXmlFormat = new ConcurrentDictionary<string, string>();

        private static readonly object BackupLock = new object();

        /// <summary>
        /// Returns a XML scenario in the standard KSP format
        /// </summary>
        public static string GetScenarioInConfigNodeFormat(string scenarioName)
        {
            return CurrentScenariosInXmlFormat.TryGetValue(scenarioName, out var scenarioInXmlFormat) ?
                ConfigNodeXmlParser.ConvertToConfigNode(scenarioInXmlFormat) : null;
        }

        /// <summary>
        /// Load the stored scenarios into the dictionary
        /// </summary>
        public static void LoadExistingScenarios()
        {
            lock (BackupLock)
            {
                foreach (var file in Directory.GetFiles(ScenariosFolder).Where(f => Path.GetExtension(f) == ".xml"))
                {
                    CurrentScenariosInXmlFormat.TryAdd(Path.GetFileNameWithoutExtension(file), FileHandler.ReadFileText(file));
                }
            }
        }

        /// <summary>
        /// Actually performs the backup of the scenarios to file
        /// </summary>
        public static void BackupScenarios()
        {
            lock (BackupLock)
            {
                var scenariosInXml = CurrentScenariosInXmlFormat.ToArray();
                foreach (var scenario in scenariosInXml)
                {
                    FileHandler.WriteToFile(Path.Combine(ScenariosFolder, $"{scenario.Key}.xml"), scenario.Value);
                }
            }
        }
    }
}

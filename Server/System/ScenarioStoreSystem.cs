using LunaConfigNode;
using LunaConfigNode.CfgNode;
using Server.Settings.Structures;
using Server.System.Scenario;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;

// ReSharper disable InconsistentlySynchronizedField

namespace Server.System
{
    /// <summary>
    /// Here we keep a copy of all the scnarios modules in ConfigNode format and we also save them to files at a specified rate
    /// </summary>
    public static class ScenarioStoreSystem
    {
        public static ConcurrentDictionary<string, ConfigNode> CurrentScenarios = new ConcurrentDictionary<string, ConfigNode>();

        private static readonly object BackupLock = new object();

        /// <summary>
        /// Returns a scenario in the standard KSP format
        /// </summary>
        public static string GetScenarioInConfigNodeFormat(string scenarioName)
        {
            return CurrentScenarios.TryGetValue(scenarioName, out var scenario) ?
                scenario.ToString() : null;
        }

        /// <summary>
        /// Load the stored scenarios into the dictionary
        /// </summary>
        public static void LoadExistingScenarios(bool createdFromScratch)
        {
            ChangeExistingScenarioFormats();
            lock (BackupLock)
            {
                foreach (var file in Directory.GetFiles(ScenarioSystem.ScenariosPath).Where(f => Path.GetExtension(f) == ScenarioSystem.ScenarioFileFormat))
                {
                    CurrentScenarios.TryAdd(Path.GetFileNameWithoutExtension(file), new ConfigNode(File.ReadAllText(file)));
                }

                if (createdFromScratch)
                {
                    ScenarioDataUpdater.WriteScienceDataToFile(GameplaySettings.SettingsStore.StartingScience);
                    ScenarioDataUpdater.WriteReputationDataToFile(GameplaySettings.SettingsStore.StartingReputation);
                    ScenarioDataUpdater.WriteFundsDataToFile(GameplaySettings.SettingsStore.StartingFunds);
                }
            }
        }

        /// <summary>
        /// Transform OLD Xml scenarios into the new format
        /// TODO: Remove this for next version
        /// </summary>
        public static void ChangeExistingScenarioFormats()
        {
            lock (BackupLock)
            {
                foreach (var file in Directory.GetFiles(ScenarioSystem.ScenariosPath).Where(f => Path.GetExtension(f) == ".xml"))
                {
                    var vesselAsCfgNode = XmlConverter.ConvertToConfigNode(FileHandler.ReadFileText(file));
                    FileHandler.WriteToFile(file.Replace(".xml", ".txt"), vesselAsCfgNode);
                    FileHandler.FileDelete(file);
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
                var scenariosInXml = CurrentScenarios.ToArray();
                foreach (var scenario in scenariosInXml)
                {
                    FileHandler.WriteToFile(Path.Combine(ScenarioSystem.ScenariosPath, $"{scenario.Key}{ScenarioSystem.ScenarioFileFormat}"), scenario.Value.ToString());
                }
            }
        }
    }
}

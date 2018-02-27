using Server.Context;
using Server.Log;
using Server.Properties;
using System.IO;

namespace Server.System
{
    public class ScenarioSystem
    {
        public static readonly string ScenarioPath = Path.Combine(ServerContext.UniverseDirectory, "Scenarios");

        public static void GenerateDefaultScenarios()
        {
            LunaLog.Normal("Creating default scenario...");
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "CommNetScenario.txt"), Resources.CommNetScenario);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "PartUpgradeManager.txt"), Resources.PartUpgradeManager);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "ProgressTracking.txt"), Resources.ProgressTracking);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "ResourceScenario.txt"), Resources.ResourceScenario);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "ScenarioAchievements.txt"), Resources.ScenarioAchievements);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "ScenarioDestructibles.txt"), Resources.ScenarioDestructibles);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "SentinelScenario.txt"), Resources.SentinelScenario);
            FileHandler.WriteToFile(Path.Combine(ScenarioPath, "VesselRecovery.txt"), Resources.VesselRecovery);
        }
    }
}

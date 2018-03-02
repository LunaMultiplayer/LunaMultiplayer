using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using System.IO;
using System.Linq;

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

        public static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Scenarios"))
                .Select(f =>
                {
                    var data = FileHandler.ReadFile(f);
                    return new ScenarioInfo
                    {
                        Data = data,
                        NumBytes = data.Length,
                        Module = Path.GetFileNameWithoutExtension(f)
                    };
                }).ToArray();

            var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<ScenarioDataMsgData>();
            msgData.ScenariosData = scenarioDataArray;
            msgData.ScenarioCount = scenarioDataArray.Length;

            MessageQueuer.SendToClient<ScenarioSrvMsg>(client, msgData);
        }


        public static void ParseReceivedScenarioData(ClientStructure client, ScenarioBaseMsgData messageData)
        {
            var data = (ScenarioDataMsgData)messageData;
            LunaLog.Debug($"Saving {data.ScenarioCount} scenario modules from {client.PlayerName}");
            for (var i = 0; i < data.ScenarioCount; i++)
            {
                var path = Path.Combine(ServerContext.UniverseDirectory, "Scenarios", $"{data.ScenariosData[i].Module}.txt");
                FileHandler.WriteToFile(path, data.ScenariosData[i].Data, data.ScenariosData[i].NumBytes);
            }
        }
    }
}

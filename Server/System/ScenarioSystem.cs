using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using Server.Settings;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.System
{
    public class ScenarioSystem
    {
        public static readonly string ScenarioPath = Path.Combine(ServerContext.UniverseDirectory, "Scenarios");

        public static void GenerateDefaultScenarios()
        {
            LunaLog.Normal("Creating default scenario files...");

            FileHandler.CreateFile(Path.Combine(ScenarioPath, "CommNetScenario.txt"), Resources.CommNetScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "PartUpgradeManager.txt"), Resources.PartUpgradeManager);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ProgressTracking.txt"), Resources.ProgressTracking);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ResourceScenario.txt"), Resources.ResourceScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioAchievements.txt"), Resources.ScenarioAchievements);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioDestructibles.txt"), Resources.ScenarioDestructibles);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioNewGameIntro.txt"), Resources.ScenarioNewGameIntro);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "SentinelScenario.txt"), Resources.SentinelScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "VesselRecovery.txt"), Resources.VesselRecovery);
            
            if (GeneralSettings.SettingsStore.GameMode != GameMode.Sandbox)
            {
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ResearchAndDevelopment.txt"), Resources.ResearchAndDevelopment);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ResearchAndDevelopment.txt"));
            }

            if (GeneralSettings.SettingsStore.GameMode == GameMode.Career)
            {
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ContractSystem.txt"), Resources.ContractSystem);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "Funding.txt"), Resources.Funding);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "Reputation.txt"), Resources.Reputation);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioContractEvents.txt"), Resources.ScenarioContractEvents);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioUpgradeableFacilities.txt"), Resources.ScenarioUpgradeableFacilities);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "StrategySystem.txt"), Resources.StrategySystem);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ContractSystem.txt"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "Funding.txt"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "Reputation.txt"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ScenarioContractEvents.txt"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ScenarioUpgradeableFacilities.txt"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "StrategySystem.txt"));
            }
        }

        public static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = ScenarioStoreSystem.CurrentScenariosInXmlFormat.Keys.Select(s =>
            {
                var serializedData = Encoding.UTF8.GetBytes(ScenarioStoreSystem.GetScenarioInConfigNodeFormat(s));
                return new ScenarioInfo
                {
                    Data = serializedData,
                    NumBytes = serializedData.Length,
                    Module = Path.GetFileNameWithoutExtension(s)
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

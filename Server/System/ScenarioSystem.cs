using LmpCommon.Enums;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using Server.Settings.Structures;
using Server.System.Scenario;
using System.IO;
using System.Linq;
using System.Text;

namespace Server.System
{
    public class ScenarioSystem
    {
        public const string ScenarioFileFormat = ".txt";
        public static string ScenariosPath = Path.Combine(ServerContext.UniverseDirectory, "Scenarios");

        public static bool GenerateDefaultScenarios()
        {
            var scenarioFilesCreated =
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "CommNetScenario.txt"), Resources.CommNetScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "PartUpgradeManager.txt"), Resources.PartUpgradeManager) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ProgressTracking.txt"), Resources.ProgressTracking) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ResourceScenario.txt"), Resources.ResourceScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioAchievements.txt"), Resources.ScenarioAchievements) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioDestructibles.txt"), Resources.ScenarioDestructibles) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "SentinelScenario.txt"), Resources.SentinelScenario) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "VesselRecovery.txt"), Resources.VesselRecovery) &&
            FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioNewGameIntro.txt"), Resources.ScenarioNewGameIntro);

            if (GeneralSettings.SettingsStore.GameMode != GameMode.Sandbox)
            {
                scenarioFilesCreated &= FileHandler.CreateFile(Path.Combine(ScenariosPath, "ResearchAndDevelopment.txt"), Resources.ResearchAndDevelopment);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ResearchAndDevelopment.txt"));
            }

            if (GeneralSettings.SettingsStore.GameMode == GameMode.Career)
            {
                scenarioFilesCreated &=
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ContractSystem.txt"), Resources.ContractSystem) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "Funding.txt"), Resources.Funding) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "Reputation.txt"), Resources.Reputation) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioContractEvents.txt"), Resources.ScenarioContractEvents) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "ScenarioUpgradeableFacilities.txt"), Resources.ScenarioUpgradeableFacilities) &&
                FileHandler.CreateFile(Path.Combine(ScenariosPath, "StrategySystem.txt"), Resources.StrategySystem);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ContractSystem.txt"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "Funding.txt"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "Reputation.txt"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ScenarioContractEvents.txt"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "ScenarioUpgradeableFacilities.txt"));
                FileHandler.FileDelete(Path.Combine(ScenariosPath, "StrategySystem.txt"));
            }

            return scenarioFilesCreated;
        }

        public static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = ScenarioStoreSystem.CurrentScenarios.Keys.Select(s =>
            {
                var scenarioConfigNode = ScenarioStoreSystem.GetScenarioInConfigNodeFormat(s);
                var serializedData = Encoding.UTF8.GetBytes(scenarioConfigNode);
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
                var scenarioAsConfigNode = Encoding.UTF8.GetString(data.ScenariosData[i].Data, 0, data.ScenariosData[i].NumBytes);
                ScenarioDataUpdater.RawConfigNodeInsertOrUpdate(data.ScenariosData[i].Module, scenarioAsConfigNode);
            }
        }
    }
}

using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Server;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Properties;
using Server.Server;
using Server.Settings;
using Server.System.Scenario;
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
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "CommNetScenario.xml"), Resources.CommNetScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "PartUpgradeManager.xml"), Resources.PartUpgradeManager);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ProgressTracking.xml"), Resources.ProgressTracking);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ResourceScenario.xml"), Resources.ResourceScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioAchievements.xml"), Resources.ScenarioAchievements);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioDestructibles.xml"), Resources.ScenarioDestructibles);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "SentinelScenario.xml"), Resources.SentinelScenario);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "VesselRecovery.xml"), Resources.VesselRecovery);
            FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioNewGameIntro.xml"), Resources.ScenarioNewGameIntro);

            if (GeneralSettings.SettingsStore.GameMode != GameMode.Sandbox)
            {
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ResearchAndDevelopment.xml"), Resources.ResearchAndDevelopment);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ResearchAndDevelopment.xml"));
            }

            if (GeneralSettings.SettingsStore.GameMode == GameMode.Career)
            {
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ContractSystem.xml"), Resources.ContractSystem);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "Funding.xml"), Resources.Funding);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "Reputation.xml"), Resources.Reputation);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioContractEvents.xml"), Resources.ScenarioContractEvents);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "ScenarioUpgradeableFacilities.xml"), Resources.ScenarioUpgradeableFacilities);
                FileHandler.CreateFile(Path.Combine(ScenarioPath, "StrategySystem.xml"), Resources.StrategySystem);
            }
            else
            {
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ContractSystem.xml"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "Funding.xml"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "Reputation.xml"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ScenarioContractEvents.xml"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "ScenarioUpgradeableFacilities.xml"));
                FileHandler.FileDelete(Path.Combine(ScenarioPath, "StrategySystem.xml"));
            }
        }

        public static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = ScenarioStoreSystem.CurrentScenariosInXmlFormat.Keys.Select(s =>
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
            LunaLog.Debug($"Saving {data.ScenarioCount} scenario modules ({string.Join(", ", data.ScenariosData.Select(s=> s.Module))}) from {client.PlayerName}");
            for (var i = 0; i < data.ScenarioCount; i++)
            {
                var scenarioAsConfigNode = Encoding.UTF8.GetString(data.ScenariosData[i].Data, 0, data.ScenariosData[i].NumBytes);
                ScenarioDataUpdater.RawConfigNodeInsertOrUpdate(data.ScenariosData[i].Module, scenarioAsConfigNode);
            }
        }
    }
}

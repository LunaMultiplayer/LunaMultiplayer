using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Message.Reader.Base;
using LunaServer.Server;
using LunaServer.System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaServer.Message.Reader
{
    public class ScenarioDataMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var message = messageData as ScenarioBaseMsgData;
            switch (message?.ScenarioMessageType)
            {
                case ScenarioMessageType.Request:
                    SendScenarioModules(client);
                    break;
                case ScenarioMessageType.Data:
                    var data = (ScenarioDataMsgData)message;
                    LunaLog.Debug($"Saving {data.ScenarioNameData.Length} scenario modules from {client.PlayerName}");
                    foreach (var scenario in data.ScenarioNameData)
                    {
                        var path = Path.Combine(ServerContext.UniverseDirectory, "Scenarios", client.PlayerName, $"{scenario.Key}.txt");
                        FileHandler.WriteToFile(path, scenario.Value);
                    }
                    break;
            }
        }

        private static void SendScenarioModules(ClientStructure client)
        {
            var scenarioDataArray = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Scenarios", client.PlayerName))
                .Select(f => new KeyValuePair<string, byte[]>(Path.GetFileNameWithoutExtension(f), FileHandler.ReadFile(f)));

            var newMessageData = new ScenarioDataMsgData
            {
                ScenarioNameData = scenarioDataArray.ToArray()
            };

            MessageQueuer.SendToClient<ScenarioSrvMsg>(client, newMessageData);
        }
    }
}
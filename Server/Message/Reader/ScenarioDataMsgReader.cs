using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Message.Reader.Base;
using Server.Server;
using Server.System;
using System.IO;
using System.Linq;

namespace Server.Message.Reader
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
                    LunaLog.Debug($"Saving {data.ScenarioCount} scenario modules from {client.PlayerName}");
                    for (var i = 0; i < data.ScenarioCount; i++)
                    {
                        var path = Path.Combine(ServerContext.UniverseDirectory, "Scenarios", $"{data.ScenariosData[i].Module}.txt");
                        FileHandler.WriteToFile(path, data.ScenariosData[i].Data, data.ScenariosData[i].NumBytes);
                    }
                    break;
            }
        }

        private static void SendScenarioModules(ClientStructure client)
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
    }
}
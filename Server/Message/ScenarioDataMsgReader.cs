using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Interface;
using LmpCommon.Message.Types;
using Server.Client;
using Server.Log;
using Server.Message.Base;
using Server.System;

namespace Server.Message
{
    public class ScenarioDataMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as ScenarioBaseMsgData;
            switch (messageData?.ScenarioMessageType)
            {
                case ScenarioMessageType.Request:
                    LunaLog.Debug($"Scenario request from {client.PlayerName}");
                    ScenarioSystem.SendScenarioModules(client);
                    break;
                case ScenarioMessageType.Data:
                    ScenarioSystem.ParseReceivedScenarioData(client, messageData);
                    break;
            }

            //We don't use this message anymore so we can recycle it
            message.Recycle();
        }
    }
}

using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Message.Reader.Base;
using Server.System;

namespace Server.Message.Reader
{
    public class ScenarioDataMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IClientMessageBase message)
        {
            var messageData = message.Data as ScenarioBaseMsgData;
            switch (messageData?.ScenarioMessageType)
            {
                case ScenarioMessageType.Request:
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
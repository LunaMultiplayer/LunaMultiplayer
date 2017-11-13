using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using System.Collections.Generic;
using System.Linq;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioMessageSender : SubSystem<ScenarioSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ScenarioCliMsg>(msg)));
        }

        public void SendScenarioModuleData(List<string> scenarioNames, List<byte[]> scenarioData)
        {
            var data = NetworkMain.CliMsgFactory.CreateNewMessageData<ScenarioDataMsgData>();

            var list = scenarioNames.Select((t, i) => new KeyValuePair<string, byte[]>(t, scenarioData[i]));
            data.ScenarioNameData = list.ToArray();

            LunaLog.Log($"[LMP]: Sending {scenarioNames.Count} scenario modules");
            SendMessage(data);
        }
    }
}
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

        public void SendScenarioModuleData(string[] scenarioNames, byte[][] scenarioData)
        {
            var data = new ScenarioDataMsgData();
            var list = scenarioNames.Select((t, i) => new KeyValuePair<string, byte[]>(t, scenarioData[i])).ToList();
            data.ScenarioNameData = list.ToArray();

            LunaLog.Log($"[LMP]: Sending {scenarioNames.Length} scenario modules");
            SendMessage(data);
        }
    }
}
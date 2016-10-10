using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.Network;
using LunaClient.Utilities;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioMessageSender : SubSystem<ScenarioSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ScenarioCliMsg>(msg));
        }

        public void SendScenarioModuleData(string[] scenarioNames, byte[][] scenarioData)
        {
            var data = new ScenarioDataMsgData();
            var list = scenarioNames.Select((t, i) => new KeyValuePair<string, byte[]>(t, scenarioData[i])).ToList();
            data.ScenarioNameData = list.ToArray();

            Debug.Log("Sending " + scenarioNames.Length + " scenario modules");
            SendMessage(data);
        }
    }
}
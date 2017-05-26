using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioMessageHandler : SubSystem<ScenarioSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as ScenarioBaseMsgData;
            if (msgData?.ScenarioMessageType == ScenarioMessageType.Data)
            {
                var data = ((ScenarioDataMsgData)messageData).ScenarioNameData;
                foreach (var scenario in data)
                {
                    var scenarioNode = ConfigNodeSerializer.Deserialize(scenario.Value);
                    if (scenarioNode != null)
                    {
                        var entry = new ScenarioEntry
                        {
                            ScenarioName = scenario.Key,
                            ScenarioNode = scenarioNode
                        };
                        System.ScenarioQueue.Enqueue(entry);
                    }
                    else
                    {
                        Debug.LogError($"[LMP]: Scenario data has been lost for {scenario.Key}");
                        ScreenMessages.PostScreenMessage($"Scenario data has been lost for {scenario.Key}", 5f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                MainSystem.Singleton.NetworkState = ClientState.ScneariosSynced;
            }
        }
    }
}
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Utilities;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Concurrent;

namespace LunaClient.Systems.Scenario
{
    public class ScenarioMessageHandler : SubSystem<ScenarioSystem>, IMessageHandler
    {
        public ConcurrentQueue<IServerMessageBase> IncomingMessages { get; set; } = new ConcurrentQueue<IServerMessageBase>();

        public void HandleMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is ScenarioBaseMsgData msgData)) return;

            if (msgData.ScenarioMessageType == ScenarioMessageType.Data)
            {
                var data = ((ScenarioDataMsgData)msgData).ScenarioNameData;
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
                        LunaLog.LogError($"[LMP]: Scenario data has been lost for {scenario.Key}");
                        ScreenMessages.PostScreenMessage($"Scenario data has been lost for {scenario.Key}", 5f, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
                MainSystem.NetworkState = ClientState.ScneariosSynced;
            }
        }
    }
}
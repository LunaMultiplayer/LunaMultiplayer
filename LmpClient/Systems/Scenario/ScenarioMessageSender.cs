using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Interface;
using System.Collections.Generic;
using System.Linq;

namespace LmpClient.Systems.Scenario
{
    public class ScenarioMessageSender : SubSystem<ScenarioSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<ScenarioCliMsg>(msg)));
        }

        public void SendScenariosRequest()
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(NetworkMain.CliMsgFactory.CreateNew<ScenarioCliMsg, ScenarioRequestMsgData>()));
        }

        public void SendScenarioModuleData(List<string> scenarioNames, List<byte[]> scenarioData)
        {
            var data = NetworkMain.CliMsgFactory.CreateNewMessageData<ScenarioDataMsgData>();

            var scenarios = scenarioNames.Select((t, i) => new ScenarioInfo
            {
                Data = scenarioData[i],
                NumBytes = scenarioData[i].Length,
                Module = t
            }).ToArray();

            data.ScenariosData = scenarios;
            data.ScenarioCount = scenarios.Length;

            LunaLog.Log($"[LMP]: Sending {data.ScenarioCount} scenario modules");
            SendMessage(data);
        }
    }
}
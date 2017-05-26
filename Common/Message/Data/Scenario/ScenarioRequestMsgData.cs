using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioRequestMsgData : ScenarioBaseMsgData
    {
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Request;
    }
}

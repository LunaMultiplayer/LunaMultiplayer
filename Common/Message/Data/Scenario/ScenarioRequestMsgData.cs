using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Scenario
{
    public class ScenarioRequestMsgData : ScenarioBaseMsgData
    {
        /// <inheritdoc />
        internal ScenarioRequestMsgData() { }
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Request;

        public override string ClassName { get; } = nameof(ScenarioRequestMsgData);
    }
}

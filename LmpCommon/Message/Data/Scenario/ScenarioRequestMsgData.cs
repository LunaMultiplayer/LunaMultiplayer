using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Scenario
{
    public class ScenarioRequestMsgData : ScenarioBaseMsgData
    {
        /// <inheritdoc />
        internal ScenarioRequestMsgData() { }
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Request;

        public override string ClassName { get; } = nameof(ScenarioRequestMsgData);
    }
}

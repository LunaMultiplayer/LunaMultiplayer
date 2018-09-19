using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Scenario
{
    public class ScenarioProtoMsgData : ScenarioBaseMsgData
    {
        /// <inheritdoc />
        internal ScenarioProtoMsgData() { }
        public override ScenarioMessageType ScenarioMessageType => ScenarioMessageType.Proto;

        public ScenarioInfo ScenarioData = new ScenarioInfo();

        public override string ClassName { get; } = nameof(ScenarioProtoMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            ScenarioData.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            ScenarioData.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + ScenarioData.GetByteCount();
        }
    }
}

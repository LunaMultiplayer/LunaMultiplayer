using Lidgren.Network;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending strategy changes between clients.
    /// </summary>
    public class ShareProgressStrategyMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressStrategyMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.StrategyUpdate;

        public StrategyInfo Strategy = new StrategyInfo();

        public override string ClassName { get; } = nameof(ShareProgressStrategyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            Strategy.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            Strategy.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + Strategy.GetByteCount();
        }
    }
}

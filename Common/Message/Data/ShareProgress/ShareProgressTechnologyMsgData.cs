using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.ShareProgress
{
    /// <summary>
    /// Data packet for sending technology unlocks between clients.
    /// </summary>
    public class ShareProgressTechnologyMsgData : ShareProgressBaseMsgData
    {
        /// <inheritdoc />
        internal ShareProgressTechnologyMsgData() { }
        public override ShareProgressMessageType ShareProgressMessageType => ShareProgressMessageType.TechnologyUpdate;

        public TechNodeInfo TechNode = new TechNodeInfo();

        public override string ClassName { get; } = nameof(ShareProgressTechnologyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            TechNode.Serialize(lidgrenMsg);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            TechNode.Deserialize(lidgrenMsg);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + TechNode.GetByteCount();
        }
    }
}

using Lidgren.Network;
using LunaCommon.Message.Base;
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

        public string TechId;

        public override string ClassName { get; } = nameof(ShareProgressTechnologyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            lidgrenMsg.Write(TechId);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            
            TechId = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + TechId.GetByteCount();
        }
    }
}

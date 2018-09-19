using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Admin
{
    public class AdminReplyMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminReplyMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Reply;

        public AdminResponse Response;
        public override string ClassName { get; } = nameof(AdminReplyMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write((int)Response);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            Response = (AdminResponse)lidgrenMsg.ReadInt32();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(AdminResponse);
        }
    }
}

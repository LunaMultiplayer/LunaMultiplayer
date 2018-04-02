using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminBanMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminBanMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Ban;

        public string PlayerName;
        public string Reason;

        public override string ClassName { get; } = nameof(AdminBanMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(Reason);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            PlayerName = lidgrenMsg.ReadString();
            Reason = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerName.GetByteCount() + Reason.GetByteCount();
        }
    }
}

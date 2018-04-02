using Lidgren.Network;
using LunaCommon.Message.Base;

namespace LunaCommon.Message.Data.Admin
{
    public abstract class AdminBanKickMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminBanKickMsgData() { }

        public string PlayerName;
        public string Reason;
        
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

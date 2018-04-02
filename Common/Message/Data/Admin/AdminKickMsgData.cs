using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminKickMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminKickMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Kick;

        public string PlayerName;

        public override string ClassName { get; } = nameof(AdminKickMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            lidgrenMsg.Write(PlayerName);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            PlayerName = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerName.GetByteCount();
        }
    }
}

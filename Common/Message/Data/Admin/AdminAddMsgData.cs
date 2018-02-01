using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminAddMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminAddMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Add;

        public string PlayerName;

        public override string ClassName { get; } = nameof(AdminAddMsgData);

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            PlayerName = lidgrenMsg.ReadString();
        }
        
        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            lidgrenMsg.Write(PlayerName);
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerName.GetByteCount();
        }
    }
}
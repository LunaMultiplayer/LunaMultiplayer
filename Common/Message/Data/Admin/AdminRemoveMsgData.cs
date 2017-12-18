using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminRemoveMsgData : AdminBaseMsgData
    {
        /// <inheritdoc />
        internal AdminRemoveMsgData() { }
        public override AdminMessageType AdminMessageType => AdminMessageType.Remove;

        public string PlayerName;

        public override string ClassName { get; } = nameof(AdminRemoveMsgData);

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);
            PlayerName = lidgrenMsg.ReadString();
        }

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);
            lidgrenMsg.Write(PlayerName);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerName.GetByteCount();
        }
    }
}
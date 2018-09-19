using Lidgren.Network;
using LmpCommon.Message.Base;
using LmpCommon.Message.Types;

namespace LmpCommon.Message.Data.Handshake
{
    public class HandshakeRequestMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeRequestMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Request;

        public string PlayerName;
        public string UniqueIdentifier;

        public override string ClassName { get; } = nameof(HandshakeRequestMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);
            
            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(UniqueIdentifier);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            PlayerName = lidgrenMsg.ReadString();
            UniqueIdentifier = lidgrenMsg.ReadString();
        }

        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + PlayerName.GetByteCount() + UniqueIdentifier.GetByteCount();
        }
    }
}

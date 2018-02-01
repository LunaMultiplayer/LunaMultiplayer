using Lidgren.Network;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeChallengeMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeChallengeMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Challenge;

        public byte[] Challenge = new byte[1024];
        
        public override string ClassName { get; } = nameof(HandshakeChallengeMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(Challenge, 0, 1024);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);
            lidgrenMsg.ReadBytes(Challenge, 0, 1024);
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(byte) * 1024;
        }
    }
}
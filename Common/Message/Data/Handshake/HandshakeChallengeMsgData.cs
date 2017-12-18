using Lidgren.Network;
using LunaCommon.Message.Base;
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

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(Challenge, 0, 1024);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);
            
            Challenge = ArrayPool<byte>.ClaimWithExactLength(1024);
            lidgrenMsg.ReadBytes(Challenge, 0, 1024);
        }

        public override void Recycle()
        {
            base.Recycle();

            ArrayPool<byte>.Release(ref Challenge, true);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + sizeof(byte) * 1024;
        }
    }
}
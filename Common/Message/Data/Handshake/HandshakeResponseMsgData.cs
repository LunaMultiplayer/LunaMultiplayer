using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeResponseMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeResponseMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Response;

        public string PlayerName;
        public string PublicKey;

        public int NumBytes;
        public byte[] ChallengeSignature = new byte[0];

        public override string ClassName { get; } = nameof(HandshakeResponseMsgData);

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalSerialize(lidgrenMsg, dataCompressed);

            lidgrenMsg.Write(PlayerName);
            lidgrenMsg.Write(PublicKey);
            
            lidgrenMsg.Write(NumBytes);
            lidgrenMsg.Write(ChallengeSignature, 0, NumBytes);
        }

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg, bool dataCompressed)
        {
            base.InternalDeserialize(lidgrenMsg, dataCompressed);

            PlayerName = lidgrenMsg.ReadString();
            PublicKey = lidgrenMsg.ReadString();

            NumBytes = lidgrenMsg.ReadInt32();
            if (ChallengeSignature.Length < NumBytes)
                ChallengeSignature = new byte[NumBytes];

            lidgrenMsg.ReadBytes(ChallengeSignature, 0, NumBytes);
        }

        internal override int InternalGetMessageSize(bool dataCompressed)
        {
            return base.InternalGetMessageSize(dataCompressed) + PlayerName.GetByteCount() + PublicKey.GetByteCount() + sizeof(byte) * 1024;
        }
    }
}
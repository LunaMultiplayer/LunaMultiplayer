using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeResponseMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeResponseMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Response;
        public string PlayerName { get; set; }
        public string PublicKey { get; set; }
        public byte[] ChallengeSignature { get; set; }
    }
}
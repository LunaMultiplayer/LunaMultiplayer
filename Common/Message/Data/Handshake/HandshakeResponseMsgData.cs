using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeResponseMsgData : HandshakeBaseMsgData
    {
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.RESPONSE;
        public string PlayerName { get; set; }
        public string PublicKey { get; set; }
        public byte[] ChallengeSignature { get; set; }
    }
}
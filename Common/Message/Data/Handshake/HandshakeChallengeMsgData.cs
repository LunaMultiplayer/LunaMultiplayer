using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeChallengeMsgData : HandshakeBaseMsgData
    {
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.CHALLENGE;
        public byte[] Challenge { get; set; }
    }
}
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeChallengeMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeChallengeMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Challenge;
        public byte[] Challenge { get; set; }
    }
}
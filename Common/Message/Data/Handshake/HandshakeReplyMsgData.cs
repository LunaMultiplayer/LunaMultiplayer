using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Handshake
{
    public class HandshakeReplyMsgData : HandshakeBaseMsgData
    {
        /// <inheritdoc />
        internal HandshakeReplyMsgData() { }
        public override HandshakeMessageType HandshakeMessageType => HandshakeMessageType.Reply;
        public HandshakeReply Response { get; set; }
        public string Reason { get; set; }
        public ModControlMode ModControlMode { get; set; }
        public long ServerStartTime { get; set; }
        public byte[] ModFileData { get; set; }
    }
}
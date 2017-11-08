using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatLeaveMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatLeaveMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.Leave;
        public string Channel { get; set; }
    }
}
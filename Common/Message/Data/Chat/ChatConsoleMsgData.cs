using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatConsoleMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatConsoleMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ConsoleMessage;
        public string Message { get; set; }
    }
}
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatConsoleMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.CONSOLE_MESSAGE;
        public string Message { get; set; }
    }
}
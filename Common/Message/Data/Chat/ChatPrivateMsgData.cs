using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatPrivateMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.PRIVATE_MESSAGE;
        public string To { get; set; }
        public string Text { get; set; }
    }
}
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatPrivateMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.PrivateMessage;
        public string To { get; set; }
        public string Text { get; set; }
    }
}
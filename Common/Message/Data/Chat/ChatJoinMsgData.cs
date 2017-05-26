using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatJoinMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.Join;
        public string Channel { get; set; }
    }
}
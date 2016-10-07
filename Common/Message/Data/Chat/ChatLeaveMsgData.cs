using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatLeaveMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.LEAVE;
        public string Channel { get; set; }
    }
}
using LunaCommon.Enums;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatJoinMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.JOIN;
        public string Channel { get; set; }
    }
}
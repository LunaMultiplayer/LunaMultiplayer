using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatListRequestMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.ListRequest;
    }
}
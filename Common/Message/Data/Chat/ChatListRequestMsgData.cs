using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatListRequestMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatListRequestMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ListRequest;
    }
}
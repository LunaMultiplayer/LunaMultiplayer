using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatListReplyMsgData : ChatBaseMsgData
    {
        /// <inheritdoc />
        internal ChatListReplyMsgData() { }
        public override ChatMessageType ChatMessageType => ChatMessageType.ListReply;

        /// <summary>
        ///     Player  -->List of channels of this player
        ///     Player2 -->List of channels of this player
        ///     ...
        /// </summary>
        public KeyValuePair<string, string[]>[] PlayerChannels { get; set; }
    }
}
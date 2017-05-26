using System.Collections.Generic;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Chat
{
    public class ChatListReplyMsgData : ChatBaseMsgData
    {
        public override ChatMessageType ChatMessageType => ChatMessageType.ListReply;

        /// <summary>
        ///     Player  -->List of channels of this player
        ///     Player2 -->List of channels of this player
        ///     ...
        /// </summary>
        public KeyValuePair<string, string[]>[] PlayerChannels { get; set; }
    }
}
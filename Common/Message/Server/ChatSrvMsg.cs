using System;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class ChatSrvMsg : SrvMsgBase<ChatBaseMsgData>
    {
        /// <inheritdoc />
        internal ChatSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ChatMessageType.ListReply] = typeof(ChatListReplyMsgData),
            [(ushort)ChatMessageType.Join] = typeof(ChatJoinMsgData),
            [(ushort)ChatMessageType.Leave] = typeof(ChatLeaveMsgData),
            [(ushort)ChatMessageType.ChannelMessage] = typeof(ChatChannelMsgData),
            [(ushort)ChatMessageType.PrivateMessage] = typeof(ChatPrivateMsgData),
            [(ushort)ChatMessageType.ConsoleMessage] = typeof(ChatConsoleMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Chat;
        protected override int DefaultChannel => 3;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
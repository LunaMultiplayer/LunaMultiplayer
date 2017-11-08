using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
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
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ChatMessageType.ListReply] = MessageStore.GetMessageData<ChatListReplyMsgData>(true),
            [(ushort)ChatMessageType.Join] = MessageStore.GetMessageData<ChatJoinMsgData>(true),
            [(ushort)ChatMessageType.Leave] = MessageStore.GetMessageData<ChatLeaveMsgData>(true),
            [(ushort)ChatMessageType.ChannelMessage] = MessageStore.GetMessageData<ChatChannelMsgData>(true),
            [(ushort)ChatMessageType.PrivateMessage] = MessageStore.GetMessageData<ChatPrivateMsgData>(true),
            [(ushort)ChatMessageType.ConsoleMessage] = MessageStore.GetMessageData<ChatConsoleMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Chat;
        protected override int DefaultChannel => 3;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class ChatSrvMsg : SrvMsgBase<ChatBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ChatMessageType.ListReply] = new ChatListReplyMsgData(),
            [(ushort)ChatMessageType.Join] = new ChatJoinMsgData(),
            [(ushort)ChatMessageType.Leave] = new ChatLeaveMsgData(),
            [(ushort)ChatMessageType.ChannelMessage] = new ChatChannelMsgData(),
            [(ushort)ChatMessageType.PrivateMessage] = new ChatPrivateMsgData(),
            [(ushort)ChatMessageType.ConsoleMessage] = new ChatConsoleMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Chat;
        protected override int DefaultChannel => 3;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
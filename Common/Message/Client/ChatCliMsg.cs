using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class ChatCliMsg : CliMsgBase<ChatBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {        
            [(ushort)ChatMessageType.LIST_REQUEST] = new ChatListRequestMsgData(),
            [(ushort)ChatMessageType.JOIN] = new ChatJoinMsgData(),
            [(ushort)ChatMessageType.LEAVE] = new ChatLeaveMsgData(),
            [(ushort)ChatMessageType.CHANNEL_MESSAGE] = new ChatChannelMsgData(),
            [(ushort)ChatMessageType.PRIVATE_MESSAGE] = new ChatPrivateMsgData(),
            [(ushort)ChatMessageType.CONSOLE_MESSAGE] = new ChatConsoleMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.CHAT;

        protected override int DefaultChannel => 3;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
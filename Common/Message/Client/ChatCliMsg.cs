using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class ChatCliMsg : CliMsgBase<ChatBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {        
            [(ushort)ChatMessageType.ListRequest] = new ChatListRequestMsgData(),
            [(ushort)ChatMessageType.Join] = new ChatJoinMsgData(),
            [(ushort)ChatMessageType.Leave] = new ChatLeaveMsgData(),
            [(ushort)ChatMessageType.ChannelMessage] = new ChatChannelMsgData(),
            [(ushort)ChatMessageType.PrivateMessage] = new ChatPrivateMsgData(),
            [(ushort)ChatMessageType.ConsoleMessage] = new ChatConsoleMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Chat;

        protected override int DefaultChannel => 3;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
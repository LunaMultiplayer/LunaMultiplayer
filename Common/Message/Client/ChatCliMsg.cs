using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class ChatCliMsg : CliMsgBase<ChatBaseMsgData>
    {        
        /// <inheritdoc />
        internal ChatCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {        
            [(ushort)ChatMessageType.ListRequest] = MessageStore.GetMessageData<ChatListRequestMsgData>(true),
            [(ushort)ChatMessageType.Join] = MessageStore.GetMessageData<ChatJoinMsgData>(true),
            [(ushort)ChatMessageType.Leave] = MessageStore.GetMessageData<ChatLeaveMsgData>(true),
            [(ushort)ChatMessageType.ChannelMessage] = MessageStore.GetMessageData<ChatChannelMsgData>(true),
            [(ushort)ChatMessageType.PrivateMessage] = MessageStore.GetMessageData<ChatPrivateMsgData>(true),
            [(ushort)ChatMessageType.ConsoleMessage] = MessageStore.GetMessageData<ChatConsoleMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Chat;

        protected override int DefaultChannel => 3;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class ChatCliMsg : CliMsgBase<ChatBaseMsgData>
    {        
        /// <inheritdoc />
        internal ChatCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ChatCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {        
            [(ushort)ChatMessageType.ListRequest] = typeof(ChatListRequestMsgData),
            [(ushort)ChatMessageType.Join] = typeof(ChatJoinMsgData),
            [(ushort)ChatMessageType.Leave] = typeof(ChatLeaveMsgData),
            [(ushort)ChatMessageType.ChannelMessage] = typeof(ChatChannelMsgData),
            [(ushort)ChatMessageType.PrivateMessage] = typeof(ChatPrivateMsgData),
            [(ushort)ChatMessageType.ConsoleMessage] = typeof(ChatConsoleMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Chat;

        protected override int DefaultChannel => 3;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
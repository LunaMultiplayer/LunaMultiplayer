using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class HandshakeSrvMsg : SrvMsgBase<HandshakeBaseMsgData>
    {
        /// <inheritdoc />
        internal HandshakeSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)HandshakeMessageType.Challenge] = MessageStore.GetMessageData<HandshakeChallengeMsgData>(true),
            [(ushort)HandshakeMessageType.Reply] = MessageStore.GetMessageData<HandshakeReplyMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Handshake;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
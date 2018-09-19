using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Handshake;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class HandshakeSrvMsg : SrvMsgBase<HandshakeBaseMsgData>
    {
        /// <inheritdoc />
        internal HandshakeSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(HandshakeSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)HandshakeMessageType.Reply] = typeof(HandshakeReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Handshake;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}

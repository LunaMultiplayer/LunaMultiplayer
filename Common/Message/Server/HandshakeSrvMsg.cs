using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
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

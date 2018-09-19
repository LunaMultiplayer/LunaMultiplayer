using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Handshake;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class HandshakeCliMsg : CliMsgBase<HandshakeBaseMsgData>
    {
        /// <inheritdoc />
        internal HandshakeCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(HandshakeCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)HandshakeMessageType.Request] = typeof(HandshakeRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Handshake;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}

using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
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
            [(ushort)HandshakeMessageType.Request] = typeof(HandshakeRequestMsgData),
            [(ushort)HandshakeMessageType.Response] = typeof(HandshakeResponseMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Handshake;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
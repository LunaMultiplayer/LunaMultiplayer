using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class HandshakeCliMsg : CliMsgBase<HandshakeBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)HandshakeMessageType.REQUEST] = new HandshakeRequestMsgData(),
            [(ushort)HandshakeMessageType.RESPONSE] = new HandshakeResponseMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.HANDSHAKE;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
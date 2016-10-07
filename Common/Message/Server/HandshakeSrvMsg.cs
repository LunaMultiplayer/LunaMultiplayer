using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Handshake;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class HandshakeSrvMsg : SrvMsgBase<HandshakeBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)HandshakeMessageType.CHALLENGE] = new HandshakeChallengeMsgData(),
            [(ushort)HandshakeMessageType.REPLY] = new HandshakeReplyMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.HANDSHAKE;

        protected override int DefaultChannel => 1;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
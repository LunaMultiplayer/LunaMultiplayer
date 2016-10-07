using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class MotdSrvMsg : SrvMsgBase<MotdBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)MotdMessageType.REPLY] = new MotdReplyMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.MOTD;
        protected override int DefaultChannel => 12;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
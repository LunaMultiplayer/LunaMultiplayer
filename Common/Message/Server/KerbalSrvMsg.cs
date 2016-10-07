using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class KerbalSrvMsg : SrvMsgBase<KerbalBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)KerbalMessageType.REPLY] = new KerbalReplyMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.KERBAL;
        protected override int DefaultChannel => 7;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
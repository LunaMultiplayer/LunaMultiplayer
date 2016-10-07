using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class KerbalCliMsg : CliMsgBase<KerbalBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)KerbalMessageType.REQUEST] = new KerbalsRequestMsgData(),
            [(ushort)KerbalMessageType.PROTO] = new KerbalProtoMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.KERBAL;
        protected override int DefaultChannel => 7;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
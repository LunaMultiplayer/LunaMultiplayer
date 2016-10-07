using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class AdminCliMsg : CliMsgBase<AdminBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {        
            [(ushort)AdminMessageType.LIST_REQUEST] = new AdminListRequestMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.ADMIN;

        protected override int DefaultChannel => 16;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<AdminBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)AdminMessageType.LIST_REPLY] = new AdminListReplyMsgData(),
            [(ushort)AdminMessageType.ADD] = new AdminAddMsgData(),
            [(ushort)AdminMessageType.REMOVE] = new AdminRemoveMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.ADMIN;
        protected override int DefaultChannel => 16;
        //Must arrive but only the latest is important
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
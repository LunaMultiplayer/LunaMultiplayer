using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<AdminBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)AdminMessageType.ListReply] = new AdminListReplyMsgData(),
            [(ushort)AdminMessageType.Add] = new AdminAddMsgData(),
            [(ushort)AdminMessageType.Remove] = new AdminRemoveMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Admin;
        protected override int DefaultChannel => 16;
        //Must arrive but only the latest is important
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
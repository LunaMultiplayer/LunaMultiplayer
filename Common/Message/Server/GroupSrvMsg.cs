using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class GroupSrvMsg : SrvMsgBase<GroupBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)GroupMessageType.ListRequest] = new GroupListRequestMsgData(),
            [(ushort)GroupMessageType.ListResponse] = new GroupListResponseMsgData(),
            [(ushort)GroupMessageType.CreateGroup] = new GroupCreateMsgData(),
            [(ushort)GroupMessageType.RemoveGroup] = new GroupRemoveMsgData(),
            [(ushort)GroupMessageType.GroupUpdate] = new GroupUpdateMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Groups;

        protected override int DefaultChannel => 18;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
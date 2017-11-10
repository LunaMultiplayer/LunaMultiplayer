using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class GroupSrvMsg : SrvMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)GroupMessageType.ListRequest] = typeof(GroupListRequestMsgData),
            [(ushort)GroupMessageType.ListResponse] = typeof(GroupListResponseMsgData),
            [(ushort)GroupMessageType.CreateGroup] = typeof(GroupCreateMsgData),
            [(ushort)GroupMessageType.RemoveGroup] = typeof(GroupRemoveMsgData),
            [(ushort)GroupMessageType.GroupUpdate] = typeof(GroupUpdateMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Groups;

        protected override int DefaultChannel => 18;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
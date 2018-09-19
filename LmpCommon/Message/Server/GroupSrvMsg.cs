using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Groups;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class GroupSrvMsg : SrvMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(GroupSrvMsg);

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
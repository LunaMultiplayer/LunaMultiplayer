using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Groups;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class GroupCliMsg : CliMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(GroupCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)GroupMessageType.ListRequest] = typeof(GroupListRequestMsgData),
            [(ushort)GroupMessageType.ListResponse] = typeof(GroupListResponseMsgData),
            [(ushort)GroupMessageType.CreateGroup] = typeof(GroupCreateMsgData),
            [(ushort)GroupMessageType.RemoveGroup] = typeof(GroupRemoveMsgData),
            [(ushort)GroupMessageType.GroupUpdate] = typeof(GroupUpdateMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Groups;

        protected override int DefaultChannel => 17;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
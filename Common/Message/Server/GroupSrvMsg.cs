using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class GroupSrvMsg : SrvMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)GroupMessageType.ListRequest] = MessageStore.GetMessageData<GroupListRequestMsgData>(true),
            [(ushort)GroupMessageType.ListResponse] = MessageStore.GetMessageData<GroupListResponseMsgData>(true),
            [(ushort)GroupMessageType.CreateGroup] = MessageStore.GetMessageData<GroupCreateMsgData>(true),
            [(ushort)GroupMessageType.RemoveGroup] = MessageStore.GetMessageData<GroupRemoveMsgData>(true),
            [(ushort)GroupMessageType.GroupUpdate] = MessageStore.GetMessageData<GroupUpdateMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Groups;

        protected override int DefaultChannel => 18;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
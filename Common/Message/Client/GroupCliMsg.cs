using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class GroupCliMsg : CliMsgBase<GroupBaseMsgData>
    {
        /// <inheritdoc />
        internal GroupCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)GroupMessageType.ListRequest] = MessageStore.GetMessageData<GroupListRequestMsgData>(true),
            [(ushort)GroupMessageType.ListResponse] = MessageStore.GetMessageData<GroupListResponseMsgData>(true),
            [(ushort)GroupMessageType.CreateGroup] = MessageStore.GetMessageData<GroupCreateMsgData>(true),
            [(ushort)GroupMessageType.RemoveGroup] = MessageStore.GetMessageData<GroupRemoveMsgData>(true),
            [(ushort)GroupMessageType.GroupUpdate] = MessageStore.GetMessageData<GroupUpdateMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Groups;

        protected override int DefaultChannel => 17;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
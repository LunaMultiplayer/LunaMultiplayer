using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class GroupCliMsg : CliMsgBase<GroupBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)GroupMessageType.ListRequest] = new GroupListRequestMsgData(),
            [(ushort)GroupMessageType.ListResponse] = new GroupListResponseMsgData(),
            [(ushort)GroupMessageType.CreateGroup] = new GroupCreateMsgData(),
            [(ushort)GroupMessageType.RemoveGroup] = new GroupRemoveMsgData(),
            [(ushort)GroupMessageType.GroupUpdate] = new GroupUpdateMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Groups;

        protected override int DefaultChannel => 17;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
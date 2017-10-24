using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class GroupSrvMsg : SrvMsgBase<GroupBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)GroupMessageType.ListResponse] = new GroupListResponseMsgData(),
            [(ushort)GroupMessageType.UpdateResponse] = new GroupUpdateResponseMsgData(),
            [(ushort)GroupMessageType.Accept] = new GroupAcceptMsgData(),
            [(ushort)GroupMessageType.Add] = new GroupAddMsgData(),
            [(ushort)GroupMessageType.Invite] = new GroupInviteMsgData(),
            [(ushort)GroupMessageType.Kick] = new GroupKickMsgData(),
            [(ushort)GroupMessageType.Remove] = new GroupRemoveMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Groups;

        protected override int DefaultChannel => 18;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
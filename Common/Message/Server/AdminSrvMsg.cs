using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<AdminBaseMsgData>
    {
        /// <inheritdoc />
        internal AdminSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)AdminMessageType.ListReply] = MessageStore.GetMessageData<AdminListReplyMsgData>(true),
            [(ushort)AdminMessageType.Add] = MessageStore.GetMessageData<AdminAddMsgData>(true),
            [(ushort)AdminMessageType.Remove] = MessageStore.GetMessageData<AdminRemoveMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Admin;
        protected override int DefaultChannel => 16;
        //Must arrive but only the latest is important
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Data.Chat;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<ChatMsgData>
    {
        /// <inheritdoc />
        internal AdminSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(AdminSrvMsg);

        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)AdminMessageType.Reply] = typeof(AdminReplyMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Admin;
        protected override int DefaultChannel => 16;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}

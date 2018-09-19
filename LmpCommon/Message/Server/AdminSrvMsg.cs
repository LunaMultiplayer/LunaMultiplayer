using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Admin;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<AdminBaseMsgData>
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

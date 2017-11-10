using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Motd;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class MotdSrvMsg : SrvMsgBase<MotdBaseMsgData>
    {
        /// <inheritdoc />
        internal MotdSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)MotdMessageType.Reply] = typeof(MotdReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Motd;
        protected override int DefaultChannel => 12;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
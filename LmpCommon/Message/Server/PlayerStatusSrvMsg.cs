using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.PlayerStatus;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class PlayerStatusSrvMsg : SrvMsgBase<PlayerStatusBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerStatusSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(PlayerStatusSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)PlayerStatusMessageType.Reply] = typeof(PlayerStatusReplyMsgData),
            [(ushort)PlayerStatusMessageType.Set] = typeof(PlayerStatusSetMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.PlayerStatus;
        protected override int DefaultChannel => 4;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
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
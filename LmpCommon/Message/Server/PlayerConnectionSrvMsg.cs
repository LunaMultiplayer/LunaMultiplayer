using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.PlayerConnection;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class PlayerConnectionSrvMsg : SrvMsgBase<PlayerConnectionBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerConnectionSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(PlayerConnectionSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)PlayerConnectionMessageType.Join] = typeof(PlayerConnectionJoinMsgData),
            [(ushort)PlayerConnectionMessageType.Leave] = typeof(PlayerConnectionLeaveMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.PlayerConnection;
        protected override int DefaultChannel => 17;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
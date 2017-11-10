using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class PlayerConnectionSrvMsg : SrvMsgBase<PlayerConnectionBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerConnectionSrvMsg() { }

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
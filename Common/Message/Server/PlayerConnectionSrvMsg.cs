using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class PlayerConnectionSrvMsg : SrvMsgBase<PlayerConnectionBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerConnectionMessageType.JOIN] = new PlayerConnectionJoinMsgData(),
            [(ushort)PlayerConnectionMessageType.LEAVE] = new PlayerConnectionLeaveMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.PLAYER_CONNECTION;
        protected override int DefaultChannel => 17;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
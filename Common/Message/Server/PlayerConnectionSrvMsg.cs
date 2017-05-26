using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerConnection;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class PlayerConnectionSrvMsg : SrvMsgBase<PlayerConnectionBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerConnectionMessageType.Join] = new PlayerConnectionJoinMsgData(),
            [(ushort)PlayerConnectionMessageType.Leave] = new PlayerConnectionLeaveMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.PlayerConnection;
        protected override int DefaultChannel => 17;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
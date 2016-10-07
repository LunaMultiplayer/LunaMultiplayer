using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class PlayerColorSrvMsg : SrvMsgBase<PlayerColorBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerColorMessageType.REPLY] = new PlayerColorReplyMsgData(),
            [(ushort)PlayerColorMessageType.SET] = new PlayerColorSetMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.PLAYER_COLOR;
        protected override int DefaultChannel => 5;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
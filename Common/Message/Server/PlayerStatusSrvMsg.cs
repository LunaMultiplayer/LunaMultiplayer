using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class PlayerStatusSrvMsg : SrvMsgBase<PlayerStatusBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerStatusMessageType.REPLY] = new PlayerStatusReplyMsgData(),
            [(ushort)PlayerStatusMessageType.SET] = new PlayerStatusSetMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.PLAYER_STATUS;
        protected override int DefaultChannel => 4;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class PlayerColorCliMsg : CliMsgBase<PlayerColorBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerColorMessageType.REQUEST] = new PlayerColorRequestMsgData(),
            [(ushort)PlayerColorMessageType.SET] = new PlayerColorSetMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.PLAYER_COLOR;
        protected override int DefaultChannel => 5;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
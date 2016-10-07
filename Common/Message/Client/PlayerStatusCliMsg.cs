using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class PlayerStatusCliMsg : CliMsgBase<PlayerStatusBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerStatusMessageType.REQUEST] = new PlayerStatusRequestMsgData(),
            [(ushort)PlayerStatusMessageType.SET] = new PlayerStatusSetMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.PLAYER_STATUS;
        protected override int DefaultChannel => 4;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
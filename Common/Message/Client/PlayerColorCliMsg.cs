using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class PlayerColorCliMsg : CliMsgBase<PlayerColorBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerColorCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)PlayerColorMessageType.Request] = MessageStore.GetMessageData<PlayerColorRequestMsgData>(true),
            [(ushort)PlayerColorMessageType.Set] = MessageStore.GetMessageData<PlayerColorSetMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.PlayerColor;
        protected override int DefaultChannel => 5;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
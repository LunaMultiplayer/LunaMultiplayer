using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Color;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class PlayerColorCliMsg : CliMsgBase<PlayerColorBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerColorCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(PlayerColorCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)PlayerColorMessageType.Request] = typeof(PlayerColorRequestMsgData),
            [(ushort)PlayerColorMessageType.Set] = typeof(PlayerColorSetMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.PlayerColor;
        protected override int DefaultChannel => 5;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
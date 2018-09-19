using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.PlayerStatus;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class PlayerStatusCliMsg : CliMsgBase<PlayerStatusBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerStatusCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(PlayerStatusCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)PlayerStatusMessageType.Request] = typeof(PlayerStatusRequestMsgData),
            [(ushort)PlayerStatusMessageType.Set] = typeof(PlayerStatusSetMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.PlayerStatus;
        protected override int DefaultChannel => 4;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.PlayerStatus;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
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
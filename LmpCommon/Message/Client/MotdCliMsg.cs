using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Motd;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class MotdCliMsg : CliMsgBase<MotdBaseMsgData>
    {
        /// <inheritdoc />
        internal MotdCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(MotdCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)MotdMessageType.Request] = typeof(MotdRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Motd;
        protected override int DefaultChannel => 12;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
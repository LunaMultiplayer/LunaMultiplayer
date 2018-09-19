using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class LockCliMsg : CliMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(LockCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)LockMessageType.ListRequest] = typeof(LockListRequestMsgData),
            [(ushort)LockMessageType.Acquire] = typeof(LockAcquireMsgData),
            [(ushort)LockMessageType.Release] = typeof(LockReleaseMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
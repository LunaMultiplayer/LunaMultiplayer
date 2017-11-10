using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class LockCliMsg : CliMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockCliMsg() { }

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
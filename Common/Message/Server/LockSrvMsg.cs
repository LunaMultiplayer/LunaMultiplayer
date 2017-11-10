using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class LockSrvMsg : SrvMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)LockMessageType.ListReply] = typeof(LockListReplyMsgData),
            [(ushort)LockMessageType.Acquire] = typeof(LockAcquireMsgData),
            [(ushort)LockMessageType.Release] = typeof(LockReleaseMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Lock;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class LockSrvMsg : SrvMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(LockSrvMsg);

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
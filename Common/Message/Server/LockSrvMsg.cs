using System;
using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class LockSrvMsg : SrvMsgBase<LockBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.LIST_REPLY] = new LockListReplyMsgData(),
            [(ushort)LockMessageType.ACQUIRE] = new LockAcquireMsgData(),
            [(ushort)LockMessageType.RELEASE] = new LockReleaseMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.LOCK;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class LockSrvMsg : SrvMsgBase<LockBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.ListReply] = new LockListReplyMsgData(),
            [(ushort)LockMessageType.Acquire] = new LockAcquireMsgData(),
            [(ushort)LockMessageType.Release] = new LockReleaseMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
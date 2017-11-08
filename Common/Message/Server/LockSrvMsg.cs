using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class LockSrvMsg : SrvMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.ListReply] = MessageStore.GetMessageData<LockListReplyMsgData>(true),
            [(ushort)LockMessageType.Acquire] = MessageStore.GetMessageData<LockAcquireMsgData>(true),
            [(ushort)LockMessageType.Release] = MessageStore.GetMessageData<LockReleaseMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
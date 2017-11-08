using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class LockCliMsg : CliMsgBase<LockBaseMsgData>
    {
        /// <inheritdoc />
        internal LockCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.ListRequest] = MessageStore.GetMessageData<LockListRequestMsgData>(true),
            [(ushort)LockMessageType.Acquire] = MessageStore.GetMessageData<LockAcquireMsgData>(true),
            [(ushort)LockMessageType.Release] = MessageStore.GetMessageData<LockReleaseMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
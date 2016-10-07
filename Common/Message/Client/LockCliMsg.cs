using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class LockCliMsg : CliMsgBase<LockBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.LIST_REQUEST] = new LockListRequestMsgData(),
            [(ushort)LockMessageType.ACQUIRE] = new LockAcquireMsgData(),
            [(ushort)LockMessageType.RELEASE] = new LockReleaseMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.LOCK;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
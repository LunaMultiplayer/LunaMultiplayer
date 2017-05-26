using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Lock;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class LockCliMsg : CliMsgBase<LockBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)LockMessageType.ListRequest] = new LockListRequestMsgData(),
            [(ushort)LockMessageType.Acquire] = new LockAcquireMsgData(),
            [(ushort)LockMessageType.Release] = new LockReleaseMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Lock;
        protected override int DefaultChannel => 14;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
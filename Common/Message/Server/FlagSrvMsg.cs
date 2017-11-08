using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class FlagSrvMsg : SrvMsgBase<FlagBaseMsgData>
    {
        /// <inheritdoc />
        internal FlagSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)FlagMessageType.ListRequest] = MessageStore.GetMessageData<FlagListRequestMsgData>(true),
            [(ushort)FlagMessageType.ListResponse] = MessageStore.GetMessageData<FlagListResponseMsgData>(true),
            [(ushort)FlagMessageType.FlagData] = MessageStore.GetMessageData<FlagDataMsgData>(true),
            [(ushort)FlagMessageType.FlagDelete] = MessageStore.GetMessageData<FlagDeleteMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
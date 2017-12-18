using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class FlagSrvMsg : SrvMsgBase<FlagBaseMsgData>
    {
        /// <inheritdoc />
        internal FlagSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(FlagSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FlagMessageType.ListRequest] = typeof(FlagListRequestMsgData),
            [(ushort)FlagMessageType.ListResponse] = typeof(FlagListResponseMsgData),
            [(ushort)FlagMessageType.FlagData] = typeof(FlagDataMsgData),
            [(ushort)FlagMessageType.FlagDelete] = typeof(FlagDeleteMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
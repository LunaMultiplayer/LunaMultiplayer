using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
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
            [(ushort)FlagMessageType.FlagData] = typeof(FlagDataMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
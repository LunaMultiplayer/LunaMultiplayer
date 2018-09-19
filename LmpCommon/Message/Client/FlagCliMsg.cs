using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Flag;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class FlagCliMsg : CliMsgBase<FlagBaseMsgData>
    {
        /// <inheritdoc />
        internal FlagCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(FlagCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FlagMessageType.ListRequest] = typeof(FlagListRequestMsgData),
            [(ushort)FlagMessageType.ListResponse] = typeof(FlagListResponseMsgData),
            [(ushort)FlagMessageType.FlagData] = typeof(FlagDataMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
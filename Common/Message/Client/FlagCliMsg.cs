using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Flag;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
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
            [(ushort)FlagMessageType.FlagData] = typeof(FlagDataMsgData),
            [(ushort)FlagMessageType.FlagDelete] = typeof(FlagDeleteMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Flag;
        protected override int DefaultChannel => 10;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
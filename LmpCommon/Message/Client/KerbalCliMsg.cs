using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class KerbalCliMsg : CliMsgBase<KerbalBaseMsgData>
    {
        /// <inheritdoc />
        internal KerbalCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(KerbalCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)KerbalMessageType.Request] = typeof(KerbalsRequestMsgData),
            [(ushort)KerbalMessageType.Proto] = typeof(KerbalProtoMsgData),
            [(ushort)KerbalMessageType.Remove] = typeof(KerbalRemoveMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Kerbal;
        protected override int DefaultChannel => 7;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
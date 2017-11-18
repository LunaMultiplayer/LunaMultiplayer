using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class KerbalCliMsg : CliMsgBase<KerbalBaseMsgData>
    {
        /// <inheritdoc />
        internal KerbalCliMsg() { }

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
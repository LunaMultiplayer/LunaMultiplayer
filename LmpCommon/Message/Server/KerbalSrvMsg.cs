using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Kerbal;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class KerbalSrvMsg : SrvMsgBase<KerbalBaseMsgData>
    {
        /// <inheritdoc />
        internal KerbalSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(KerbalSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)KerbalMessageType.Reply] = typeof(KerbalReplyMsgData),
            [(ushort)KerbalMessageType.Proto] = typeof(KerbalProtoMsgData),
            [(ushort)KerbalMessageType.Remove] = typeof(KerbalRemoveMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Kerbal;
        protected override int DefaultChannel => 7;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
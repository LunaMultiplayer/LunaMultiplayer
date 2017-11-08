using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Kerbal;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class KerbalCliMsg : CliMsgBase<KerbalBaseMsgData>
    {
        /// <inheritdoc />
        internal KerbalCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)KerbalMessageType.Request] = MessageStore.GetMessageData<KerbalsRequestMsgData>(true),
            [(ushort)KerbalMessageType.Proto] = MessageStore.GetMessageData<KerbalProtoMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Kerbal;
        protected override int DefaultChannel => 7;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
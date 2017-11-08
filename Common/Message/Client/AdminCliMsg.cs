using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class AdminCliMsg : CliMsgBase<AdminBaseMsgData>
    {
        /// <inheritdoc />
        internal AdminCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {        
            [(ushort)AdminMessageType.ListRequest] = MessageStore.GetMessageData<AdminListRequestMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Admin;

        protected override int DefaultChannel => 16;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
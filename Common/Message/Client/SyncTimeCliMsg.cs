using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class SyncTimeCliMsg : CliMsgBase<SyncTimeBaseMsgData>
    {
        /// <inheritdoc />
        internal SyncTimeCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SyncTimeMessageType.Request] = MessageStore.GetMessageData<SyncTimeRequestMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.SyncTime;
        protected override int DefaultChannel => 0;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;

    }
}
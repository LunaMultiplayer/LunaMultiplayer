using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class SyncTimeCliMsg : CliMsgBase<SyncTimeBaseMsgData>
    {
        /// <inheritdoc />
        internal SyncTimeCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SyncTimeMessageType.Request] = typeof(SyncTimeRequestMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.SyncTime;
        protected override int DefaultChannel => 0;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;

    }
}
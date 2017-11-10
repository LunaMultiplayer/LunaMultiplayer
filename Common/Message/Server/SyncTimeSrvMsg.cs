using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class SyncTimeSrvMsg : SrvMsgBase<SyncTimeBaseMsgData>
    {
        /// <inheritdoc />
        internal SyncTimeSrvMsg() { }
        
        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)SyncTimeMessageType.Reply] = typeof(SyncTimeReplyMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.SyncTime;
        protected override int DefaultChannel => 0;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;

    }
}
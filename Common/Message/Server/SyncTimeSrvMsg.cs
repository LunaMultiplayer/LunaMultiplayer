using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class SyncTimeSrvMsg : SrvMsgBase<SyncTimeBaseMsgData>
    {
        /// <inheritdoc />
        internal SyncTimeSrvMsg() { }
        
        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SyncTimeMessageType.Reply] = MessageStore.GetMessageData<SyncTimeReplyMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.SyncTime;
        protected override int DefaultChannel => 0;

        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;

    }
}
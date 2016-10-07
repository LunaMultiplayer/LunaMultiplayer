using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class SyncTimeSrvMsg : SrvMsgBase<SyncTimeBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SyncTimeMessageType.REPLY] = new SyncTimeReplyMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.SYNC_TIME;
        protected override int DefaultChannel => 0;
        
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;
        
    }
}
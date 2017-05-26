using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class SyncTimeSrvMsg : SrvMsgBase<SyncTimeBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SyncTimeMessageType.Reply] = new SyncTimeReplyMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.SyncTime;
        protected override int DefaultChannel => 0;
        
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.UnreliableSequenced;
        
    }
}
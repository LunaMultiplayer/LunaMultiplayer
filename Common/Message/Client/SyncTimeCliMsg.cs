using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.SyncTime;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Client
{
    public class SyncTimeCliMsg : CliMsgBase<SyncTimeBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)SyncTimeMessageType.Request] = new SyncTimeRequestMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.SyncTime;
        protected override int DefaultChannel => 0;
        public override NetDeliveryMethod NetDeliveryMethod =>NetDeliveryMethod.UnreliableSequenced;
        
    }
}
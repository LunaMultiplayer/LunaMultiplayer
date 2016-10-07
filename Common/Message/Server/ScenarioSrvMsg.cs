using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class ScenarioSrvMsg : SrvMsgBase<ScenarioBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ScenarioMessageType.DATA] = new ScenarioDataMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.SCENARIO;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
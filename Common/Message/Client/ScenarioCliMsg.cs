using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class ScenarioCliMsg : CliMsgBase<ScenarioBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ScenarioMessageType.REQUEST] = new ScenarioRequestMsgData(),
            [(ushort)ScenarioMessageType.DATA] = new ScenarioDataMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.SCENARIO;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
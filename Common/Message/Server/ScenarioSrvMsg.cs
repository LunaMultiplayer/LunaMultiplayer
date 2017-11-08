using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class ScenarioSrvMsg : SrvMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ScenarioMessageType.Data] = MessageStore.GetMessageData<ScenarioDataMsgData>(true)
        };

        public override ServerMessageType MessageType => ServerMessageType.Scenario;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
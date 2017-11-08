using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class ScenarioCliMsg : CliMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)ScenarioMessageType.Request] = MessageStore.GetMessageData<ScenarioRequestMsgData>(true),
            [(ushort)ScenarioMessageType.Data] = MessageStore.GetMessageData<ScenarioDataMsgData>(true)
        };

        public override ClientMessageType MessageType => ClientMessageType.Scenario;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
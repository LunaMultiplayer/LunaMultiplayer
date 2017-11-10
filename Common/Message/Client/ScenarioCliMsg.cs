using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Scenario;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class ScenarioCliMsg : CliMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScenarioMessageType.Request] = typeof(ScenarioRequestMsgData),
            [(ushort)ScenarioMessageType.Data] = typeof(ScenarioDataMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Scenario;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
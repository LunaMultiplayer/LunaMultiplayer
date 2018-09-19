using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class ScenarioCliMsg : CliMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ScenarioCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScenarioMessageType.Request] = typeof(ScenarioRequestMsgData),
            [(ushort)ScenarioMessageType.Data] = typeof(ScenarioDataMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Scenario;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
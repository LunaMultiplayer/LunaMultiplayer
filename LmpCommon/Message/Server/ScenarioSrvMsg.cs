using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Scenario;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class ScenarioSrvMsg : SrvMsgBase<ScenarioBaseMsgData>
    {
        /// <inheritdoc />
        internal ScenarioSrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(ScenarioSrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)ScenarioMessageType.Data] = typeof(ScenarioDataMsgData),
            [(ushort)ScenarioMessageType.Proto] = typeof(ScenarioProtoMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Scenario;
        protected override int DefaultChannel => 6;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}
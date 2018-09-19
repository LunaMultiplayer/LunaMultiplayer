using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Data.Facility;
using LmpCommon.Message.Server.Base;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Server
{
    public class FacilitySrvMsg : SrvMsgBase<FacilityBaseMsgData>
    {
        /// <inheritdoc />
        internal FacilitySrvMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(FacilitySrvMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FacilityMessageType.Collapse] = typeof(FacilityCollapseMsgData),
            [(ushort)FacilityMessageType.Repair] = typeof(FacilityRepairMsgData),
        };

        public override ServerMessageType MessageType => ServerMessageType.Facility;
        protected override int DefaultChannel => 19;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}

using Lidgren.Network;
using LmpCommon.Enums;
using LmpCommon.Message.Client.Base;
using LmpCommon.Message.Data.Facility;
using LmpCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LmpCommon.Message.Client
{
    public class FacilityCliMsg : CliMsgBase<FacilityBaseMsgData>
    {
        /// <inheritdoc />
        internal FacilityCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(FacilityCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)FacilityMessageType.Collapse] = typeof(FacilityCollapseMsgData),
            [(ushort)FacilityMessageType.Repair] = typeof(FacilityRepairMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Facility;
        protected override int DefaultChannel => 18;
        public override NetDeliveryMethod NetDeliveryMethod => NetDeliveryMethod.ReliableOrdered;
    }
}

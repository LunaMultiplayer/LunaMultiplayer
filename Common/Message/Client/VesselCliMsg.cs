using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class VesselCliMsg : CliMsgBase<VesselBaseMsgData>
    {
        /// <inheritdoc />
        internal VesselCliMsg() { }

        /// <inheritdoc />
        public override string ClassName { get; } = nameof(VesselCliMsg);

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)VesselMessageType.Proto] = typeof(VesselProtoMsgData),
            [(ushort)VesselMessageType.Dock] = typeof(VesselDockMsgData),
            [(ushort)VesselMessageType.Remove] = typeof(VesselRemoveMsgData),
            [(ushort)VesselMessageType.Position] = typeof(VesselPositionMsgData),
            [(ushort)VesselMessageType.Flightstate] = typeof(VesselFlightStateMsgData),
            [(ushort)VesselMessageType.Update] = typeof(VesselUpdateMsgData),
            [(ushort)VesselMessageType.Resource] = typeof(VesselResourceMsgData),
            [(ushort)VesselMessageType.Sync] = typeof(VesselSyncMsgData),
            [(ushort)VesselMessageType.PartSyncField] = typeof(VesselPartSyncFieldMsgData),
            [(ushort)VesselMessageType.PartSyncCall] = typeof(VesselPartSyncCallMsgData),
            [(ushort)VesselMessageType.Fairing] = typeof(VesselFairingMsgData),
        };

        public override ClientMessageType MessageType => ClientMessageType.Vessel;
        protected override int DefaultChannel => IsUnreliableMessage() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsUnreliableMessage() ? 
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsUnreliableMessage()
        {
            return Data.SubType == (ushort) VesselMessageType.Position || Data.SubType == (ushort) VesselMessageType.Flightstate
                   || Data.SubType == (ushort)VesselMessageType.Update || Data.SubType == (ushort)VesselMessageType.Resource;
        }
    }
}
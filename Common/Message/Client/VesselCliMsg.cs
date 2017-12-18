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
            [(ushort)VesselMessageType.VesselsRequest] = typeof(VesselsRequestMsgData),
            [(ushort)VesselMessageType.Proto] = typeof(VesselProtoMsgData),
            [(ushort)VesselMessageType.ProtoReliable] = typeof(VesselProtoReliableMsgData),
            [(ushort)VesselMessageType.Dock] = typeof(VesselDockMsgData),
            [(ushort)VesselMessageType.Remove] = typeof(VesselRemoveMsgData),
            [(ushort)VesselMessageType.Position] = typeof(VesselPositionMsgData),
            [(ushort)VesselMessageType.Flightstate] = typeof(VesselFlightStateMsgData)
        };

        public override ClientMessageType MessageType => ClientMessageType.Vessel;
        protected override int DefaultChannel => IsVesselProtoPositionOrFlightState() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselProtoPositionOrFlightState() ? 
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselProtoPositionOrFlightState()
        {
            return Data.SubType == (ushort)VesselMessageType.Position || Data.SubType == (ushort)VesselMessageType.Flightstate || 
                Data.SubType == (ushort)VesselMessageType.Proto;
        }

        //Avoid compression of this message to reduce the GC
        public override bool AvoidCompression => Data.SubType == (ushort) VesselMessageType.Position || Data.SubType == (ushort)VesselMessageType.Flightstate;
    }
}
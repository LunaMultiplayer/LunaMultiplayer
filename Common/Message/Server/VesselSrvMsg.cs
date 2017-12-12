using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class VesselSrvMsg : SrvMsgBase<VesselBaseMsgData>
    {
        /// <inheritdoc />
        internal VesselSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)VesselMessageType.ListReply] = typeof(VesselListReplyMsgData),
            [(ushort)VesselMessageType.VesselsReply] = typeof(VesselsReplyMsgData),
            [(ushort)VesselMessageType.Proto] = typeof(VesselProtoMsgData),
            [(ushort)VesselMessageType.ProtoReliable] = typeof(VesselProtoReliableMsgData),
            [(ushort)VesselMessageType.Dock] = typeof(VesselDockMsgData),
            [(ushort)VesselMessageType.Remove] = typeof(VesselRemoveMsgData),
            [(ushort)VesselMessageType.Position] = typeof(VesselPositionMsgData),
            [(ushort)VesselMessageType.Flightstate] = typeof(VesselFlightStateMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Vessel;
        protected override int DefaultChannel => IsVesselProtoPositionOrFlightState() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselProtoPositionOrFlightState() ?
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselProtoPositionOrFlightState()
        {
            return Data.SubType == (ushort)VesselMessageType.Position || Data.SubType == (ushort)VesselMessageType.Flightstate || 
                Data.SubType == (ushort)VesselMessageType.Proto;
        }
    }
}
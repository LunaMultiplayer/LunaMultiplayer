using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Server
{
    public class VesselSrvMsg : SrvMsgBase<VesselBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)VesselMessageType.LIST_REPLY] = new VesselListReplyMsgData(),
            [(ushort)VesselMessageType.VESSELS_REPLY] = new VesselsReplyMsgData(),
            [(ushort)VesselMessageType.PROTO] = new VesselProtoMsgData(),
            [(ushort)VesselMessageType.UPDATE] = new VesselUpdateMsgData(),
            [(ushort)VesselMessageType.REMOVE] = new VesselRemoveMsgData(),
            [(ushort)VesselMessageType.CHANGE] = new VesselChangeMsgData(),
            [(ushort)VesselMessageType.POSITION] = new VesselPositionMsgData(),
            [(ushort)VesselMessageType.FLIGHTSTATE] = new VesselFlightStateMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.VESSEL;
        protected override int DefaultChannel => IsVesselPositionOrFlightState() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselPositionOrFlightState() ? 
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselPositionOrFlightState()
        {
            return Data.SubType == (ushort)VesselMessageType.POSITION || Data.SubType == (ushort)VesselMessageType.FLIGHTSTATE;
        }
    }
}
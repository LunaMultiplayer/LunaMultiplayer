using System.Collections.Generic;
using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Server
{
    public class VesselSrvMsg : SrvMsgBase<VesselBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)VesselMessageType.ListReply] = new VesselListReplyMsgData(),
            [(ushort)VesselMessageType.VesselsReply] = new VesselsReplyMsgData(),
            [(ushort)VesselMessageType.Proto] = new VesselProtoMsgData(),
            [(ushort)VesselMessageType.Update] = new VesselUpdateMsgData(),
            [(ushort)VesselMessageType.Remove] = new VesselRemoveMsgData(),
            [(ushort)VesselMessageType.Change] = new VesselChangeMsgData(),
            [(ushort)VesselMessageType.Position] = new VesselPositionMsgData(),
            [(ushort)VesselMessageType.Flightstate] = new VesselFlightStateMsgData()
        };

        public override ServerMessageType MessageType => ServerMessageType.Vessel;
        protected override int DefaultChannel => IsVesselPositionOrFlightState() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselPositionOrFlightState() ?
            NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselPositionOrFlightState()
        {
            return Data.SubType == (ushort)VesselMessageType.Position || Data.SubType == (ushort)VesselMessageType.Flightstate;
        }
    }
}
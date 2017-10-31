using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class VesselCliMsg : CliMsgBase<VesselBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)VesselMessageType.ListRequest] = new VesselListRequestMsgData(),
            [(ushort)VesselMessageType.VesselsRequest] = new VesselsRequestMsgData(),
            [(ushort)VesselMessageType.Proto] = new VesselProtoMsgData(),
            [(ushort)VesselMessageType.Dock] = new VesselDockMsgData(),
            [(ushort)VesselMessageType.Remove] = new VesselRemoveMsgData(),
            [(ushort)VesselMessageType.Change] = new VesselChangeMsgData(),
            [(ushort)VesselMessageType.Position] = new VesselPositionMsgData(),
            [(ushort)VesselMessageType.Flightstate] = new VesselFlightStateMsgData()
        };

        public override ClientMessageType MessageType => ClientMessageType.Vessel;
        protected override int DefaultChannel => IsVesselPositionOrFlightState() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselPositionOrFlightState() ? NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselPositionOrFlightState()
        {
            return Data.SubType == (ushort)VesselMessageType.Position || Data.SubType == (ushort)VesselMessageType.Flightstate;
        }
    }
}
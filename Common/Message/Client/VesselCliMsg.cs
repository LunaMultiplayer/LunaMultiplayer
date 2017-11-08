using Lidgren.Network;
using LunaCommon.Enums;
using LunaCommon.Message.Base;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using System.Collections.Generic;

namespace LunaCommon.Message.Client
{
    public class VesselCliMsg : CliMsgBase<VesselBaseMsgData>
    {
        /// <inheritdoc />
        internal VesselCliMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)VesselMessageType.ListRequest] = MessageStore.GetMessageData<VesselListRequestMsgData>(true),
            [(ushort)VesselMessageType.VesselsRequest] = MessageStore.GetMessageData<VesselsRequestMsgData>(true),
            [(ushort)VesselMessageType.Proto] = MessageStore.GetMessageData<VesselProtoMsgData>(true),
            [(ushort)VesselMessageType.Dock] = MessageStore.GetMessageData<VesselDockMsgData>(true),
            [(ushort)VesselMessageType.Remove] = MessageStore.GetMessageData<VesselRemoveMsgData>(true),
            [(ushort)VesselMessageType.Position] = MessageStore.GetMessageData<VesselPositionMsgData>(true),
            [(ushort)VesselMessageType.Flightstate] = MessageStore.GetMessageData<VesselFlightStateMsgData>(true)
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
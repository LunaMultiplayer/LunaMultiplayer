using System.Collections.Generic;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Types;
using Lidgren.Network;

namespace LunaCommon.Message.Client
{
    public class VesselCliMsg : CliMsgBase<VesselBaseMsgData>
    {
        protected override Dictionary<ushort, IMessageData> SubTypeDictionary { get; } = new Dictionary<ushort, IMessageData>
        {
            [(ushort)VesselMessageType.LIST_REQUEST] = new VesselListRequestMsgData(),
            [(ushort)VesselMessageType.VESSELS_REQUEST] = new VesselsRequestMsgData(),
            [(ushort)VesselMessageType.PROTO] = new VesselProtoMsgData(),
            [(ushort)VesselMessageType.UPDATE] = new VesselUpdateMsgData(),
            [(ushort)VesselMessageType.REMOVE] = new VesselRemoveMsgData(),
            [(ushort)VesselMessageType.CHANGE] = new VesselChangeMsgData(),
            [(ushort)VesselMessageType.POSITION] = new VesselPositionUpdateMsgData(),
        };

        public override ClientMessageType MessageType => ClientMessageType.VESSEL;
        protected override int DefaultChannel => IsVesselUpdate() ? 0: 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselUpdate() ? NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselUpdate()
        {
            return Data.SubType == (ushort) VesselMessageType.UPDATE;
        }
    }
}
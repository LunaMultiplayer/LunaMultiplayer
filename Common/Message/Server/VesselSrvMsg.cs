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
            [(ushort)VesselMessageType.POSITION] = new VesselPositionUpdateMsgData(),
        };

        public override ServerMessageType MessageType => ServerMessageType.VESSEL;
        protected override int DefaultChannel => IsVesselUpdate() ? 0 : 8;
        public override NetDeliveryMethod NetDeliveryMethod => IsVesselUpdate() ? NetDeliveryMethod.UnreliableSequenced : NetDeliveryMethod.ReliableOrdered;

        private bool IsVesselUpdate()
        {
            return Data.SubType == (ushort)VesselMessageType.UPDATE;
        }
    }
}
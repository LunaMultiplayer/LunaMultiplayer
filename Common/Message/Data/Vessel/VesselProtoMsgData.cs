using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;
        public int Subspace { get; set; }
        public Guid VesselId { get; set; }
        public byte[] VesselData { get; set; }
    }
}
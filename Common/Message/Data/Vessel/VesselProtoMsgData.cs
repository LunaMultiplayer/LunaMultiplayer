using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselProtoMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Proto;
        public int Subspace { get; set; }
        public Guid VesselId { get; set; }
        public byte[] VesselData { get; set; }
    }
}
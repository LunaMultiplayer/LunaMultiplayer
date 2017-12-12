using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselProtoBaseMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselProtoBaseMsgData() { }
        public int Subspace { get; set; }
        public Guid VesselId { get; set; }
        public byte[] VesselData { get; set; }
    }
}
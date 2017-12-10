using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPositionMsgData : VesselBaseMsgData
    {
        /// <inheritdoc />
        internal VesselPositionMsgData() { }
        public override VesselMessageType VesselMessageType => VesselMessageType.Position;
        public Guid VesselId { get; set; }
        public string BodyName { get; set; }
        public double[] LatLonAlt { get; set; }
        public double[] NormalVector { get; set; }
        public double[] Com { get; set; }
        public double[] TransformPosition { get; set; }
        public double[] Velocity { get; set; }
        public double[] Orbit { get; set; }
        public bool Landed { get; set; }
        public bool Splashed { get; set; }
        public float[] SrfRelRotation { get; set; }
        public long TimeStamp { get; set; }
    }
}
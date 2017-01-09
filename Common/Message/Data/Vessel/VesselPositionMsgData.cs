using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPositionMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.POSITION;
        public Guid VesselId { get; set; }
        public string BodyName { get; set; }
        public double PlanetTime { get; set; }
        public double[] LatLonAlt { get; set; }
        public double[] TransformPosition { get; set; }
        public double[] OrbitPosition { get; set; }
        public double[] Velocity { get; set; }
        public double[] OrbitVelocity { get; set; }
        public double[] Orbit { get; set; }
        public double[] Acceleration { get; set; }
        public float GameSentTime { get; set; }
        public float[] TransformRotation { get; set; }
        public Boolean Landed { get; set; }
    }
}
using LunaCommon.Message.Types;
using System;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselPositionMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.Position;
        public Guid VesselId { get; set; }
        public string BodyName { get; set; }
        public double PlanetTime { get; set; }
        public double[] LatLonAlt { get; set; }
        public double[] NormalVector { get; set; }
        public double[] Com { get; set; }
        public double[] TransformPosition { get; set; }
        public double[] OrbitPosition { get; set; }
        public double[] Velocity { get; set; }
        public double[] OrbitVelocity { get; set; }
        public double[] Orbit { get; set; }
        public double[] Acceleration { get; set; }
        public float GameSentTime { get; set; }
        public float Height { get; set; }
        public float[] TransformRotation { get; set; }
        public float[] RefTransformRot { get; set; }
        public float[] RefTransformPos { get; set; }
        public bool Landed { get; set; }
    }
}
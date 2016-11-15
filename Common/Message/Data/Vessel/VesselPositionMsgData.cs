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
        public float[] Rotation { get; set; }
        public bool IsSurfaceUpdate { get; set; }
        public double[] Position { get; set; } //Only if IsSurfaceUpdate
        public double[] Velocity { get; set; } //Only if IsSurfaceUpdate
        public double[] Orbit { get; set; } //Only if NOT IsSurfaceUpdate
        public float GameSentTime { get; set; }
    }
}
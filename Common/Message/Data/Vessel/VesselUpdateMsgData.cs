using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselUpdateMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.UPDATE;

        public double PlanetTime { get; set; }
        public Guid VesselId { get; set; }
        public int Stage { get; set; }
        public string BodyName { get; set; }
        public uint[] ActiveEngines { get; set; }
        public uint[] StoppedEngines { get; set; }
        public uint[] Decouplers { get; set; }
        public uint[] AnchoredDecouplers { get; set; }
        public uint[] Clamps { get; set; }
        public uint[] Docks { get; set; }
        public float[] Rotation { get; set; }
        public float[] AngularVelocity { get; set; }
        public float MainThrottle { get; set; }
        public float WheelThrottleTrim { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public bool KillRot { get; set; }
        public bool GearUp { get; set; }
        public bool GearDown { get; set; }
        public bool Headlight { get; set; }
        public float WheelThrottle { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float Yaw { get; set; }
        public float PitchTrim { get; set; }
        public float RollTrim { get; set; }
        public float YawTrim { get; set; }
        public float WheelSteer { get; set; }
        public float WheelSteerTrim { get; set; }
        public bool[] ActiongroupControls { get; set; }
        public bool IsSurfaceUpdate { get; set; }
        public double[] Position { get; set; } //Only if IsSurfaceUpdate
        public double[] Velocity { get; set; } //Only if IsSurfaceUpdate
        public double[] Acceleration { get; set; } //Only if IsSurfaceUpdate
        public double[] Orbit { get; set; } //Only if NOT IsSurfaceUpdate
        public long SentTime { get; set; }
    }
}
using System;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Vessel
{
    public class VesselFlightStateMsgData : VesselBaseMsgData
    {
        public override VesselMessageType VesselMessageType => VesselMessageType.FLIGHTSTATE;
        public Guid VesselId { get; set; }
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
    }
}
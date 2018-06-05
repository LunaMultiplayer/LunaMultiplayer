using LunaCommon;
using LunaCommon.Message.Data.Vessel;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public static class FlightCtrlStateExtensionMethods
    {
        public static void CopyFrom(this FlightCtrlState fs, VesselFlightStateMsgData msgData)
        {
            fs.mainThrottle = msgData.MainThrottle;
            fs.wheelThrottleTrim = msgData.WheelThrottleTrim;
            fs.X = msgData.X;
            fs.Y = msgData.Y;
            fs.Z = msgData.Z;
            fs.killRot = msgData.KillRot;
            fs.gearUp = msgData.GearUp;
            fs.gearDown = msgData.GearDown;
            fs.headlight = msgData.Headlight;
            fs.wheelThrottle = msgData.WheelThrottle;
            fs.roll = msgData.Roll;
            fs.yaw = msgData.Yaw;
            fs.pitch = msgData.Pitch;
            fs.rollTrim = msgData.RollTrim;
            fs.yawTrim = msgData.YawTrim;
            fs.pitchTrim = msgData.PitchTrim;
            fs.wheelSteer = msgData.WheelSteer;
            fs.wheelSteerTrim = msgData.WheelSteerTrim;
        }

        public static void Lerp(this FlightCtrlState fs, FlightCtrlState from, FlightCtrlState to, float lerpPercentage)
        {
            fs.X = LunaMath.Lerp(from.X, to.X, Mathf.Clamp01(lerpPercentage));
            fs.Y = LunaMath.Lerp(from.Y, to.Y, Mathf.Clamp01(lerpPercentage));
            fs.Z = LunaMath.Lerp(from.Z, to.Z, Mathf.Clamp01(lerpPercentage));
            fs.pitch = LunaMath.Lerp(from.pitch, to.pitch, Mathf.Clamp01(lerpPercentage));
            fs.pitchTrim = LunaMath.Lerp(from.pitchTrim, to.pitchTrim, Mathf.Clamp01(lerpPercentage));
            fs.roll = LunaMath.Lerp(from.roll, to.roll, Mathf.Clamp01(lerpPercentage));
            fs.rollTrim = LunaMath.Lerp(from.rollTrim, to.rollTrim, Mathf.Clamp01(lerpPercentage));
            fs.yaw = LunaMath.Lerp(from.yaw, to.yaw, Mathf.Clamp01(lerpPercentage));
            fs.yawTrim = LunaMath.Lerp(from.yawTrim, to.yawTrim, Mathf.Clamp01(lerpPercentage));
            fs.mainThrottle = LunaMath.Lerp(from.mainThrottle, to.mainThrottle, Mathf.Clamp01(lerpPercentage));
            fs.wheelSteer = LunaMath.Lerp(from.wheelSteer, to.wheelSteer, Mathf.Clamp01(lerpPercentage));
            fs.wheelSteerTrim = LunaMath.Lerp(from.wheelSteerTrim, to.wheelSteerTrim, Mathf.Clamp01(lerpPercentage));
            fs.wheelThrottle = LunaMath.Lerp(from.wheelThrottle, to.wheelThrottle, Mathf.Clamp01(lerpPercentage));
            fs.wheelThrottleTrim = LunaMath.Lerp(from.wheelThrottleTrim, to.wheelThrottleTrim, Mathf.Clamp01(lerpPercentage));
            fs.gearDown = LunaMath.Lerp(from.gearDown, to.gearDown, Mathf.Clamp01(lerpPercentage));
            fs.gearUp = LunaMath.Lerp(from.gearUp, to.gearUp, Mathf.Clamp01(lerpPercentage));
            fs.headlight = LunaMath.Lerp(from.headlight, to.headlight, Mathf.Clamp01(lerpPercentage));
            fs.killRot = LunaMath.Lerp(from.killRot, to.killRot, Mathf.Clamp01(lerpPercentage));
        }
    }
}

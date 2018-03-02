using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateUpdate
    {
        public long StartTimeStamp;
        public FlightCtrlState Start;

        public long EndTimeStamp;
        public FlightCtrlState Target;
        
        public float InterpolationDuration => (float)TimeSpan.FromTicks(EndTimeStamp - StartTimeStamp).TotalSeconds;


        private readonly FlightCtrlState _currentFlightControlState = new FlightCtrlState();
        
        private float _lerpPercentage;
        private bool _resetStart;

        public void SetTarget(VesselFlightStateMsgData msgData)
        {
            _resetStart = true;
            _lerpPercentage = 0;

            if (Target == null)
            {
                Target = GetFlightCtrlStateFromMsg(msgData);
            }
            else
            {
                Target.mainThrottle = msgData.MainThrottle;
                Target.wheelThrottleTrim = msgData.WheelThrottleTrim;
                Target.X = msgData.X;
                Target.Y = msgData.Y;
                Target.Z = msgData.Z;
                Target.killRot = msgData.KillRot;
                Target.gearUp = msgData.GearUp;
                Target.gearDown = msgData.GearDown;
                Target.headlight = msgData.Headlight;
                Target.wheelThrottle = msgData.WheelThrottle;
                Target.roll = msgData.Roll;
                Target.yaw = msgData.Yaw;
                Target.pitch = msgData.Pitch;
                Target.rollTrim = msgData.RollTrim;
                Target.yawTrim = msgData.YawTrim;
                Target.pitchTrim = msgData.PitchTrim;
                Target.wheelSteer = msgData.WheelSteer;
                Target.wheelSteerTrim = msgData.WheelSteerTrim;
            }

            StartTimeStamp = EndTimeStamp;
            EndTimeStamp = msgData.TimeStamp;
        }

        private static FlightCtrlState GetFlightCtrlStateFromMsg(VesselFlightStateMsgData msgData)
        {
            return new FlightCtrlState
            {
                mainThrottle = msgData.MainThrottle,
                wheelThrottleTrim = msgData.WheelThrottleTrim,
                X = msgData.X,
                Y = msgData.Y,
                Z = msgData.Z,
                killRot = msgData.KillRot,
                gearUp = msgData.GearUp,
                gearDown = msgData.GearDown,
                headlight = msgData.Headlight,
                wheelThrottle = msgData.WheelThrottle,
                roll = msgData.Roll,
                yaw = msgData.Yaw,
                pitch = msgData.Pitch,
                rollTrim = msgData.RollTrim,
                yawTrim = msgData.YawTrim,
                pitchTrim = msgData.PitchTrim,
                wheelSteer = msgData.WheelSteer,
                wheelSteerTrim = msgData.WheelSteerTrim
            };
        }

        public FlightCtrlState GetInterpolatedValue(FlightCtrlState currentFlightState)
        {
            if (currentFlightState == null) currentFlightState = new FlightCtrlState();

            if (_resetStart || Start == null)
            {
                _resetStart = false;
                Start = currentFlightState;
            }

            _currentFlightControlState.X = Lerp(Start.X, Target.X, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.Y = Lerp(Start.Y, Target.Y, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.Z = Lerp(Start.Z, Target.Z, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.pitch = Lerp(Start.pitch, Target.pitch, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.pitchTrim = Lerp(Start.pitchTrim, Target.pitchTrim, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.roll = Lerp(Start.roll, Target.roll, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.rollTrim = Lerp(Start.rollTrim, Target.rollTrim, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.yaw = Lerp(Start.yaw, Target.yaw, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.yawTrim = Lerp(Start.yawTrim, Target.yawTrim, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.mainThrottle = Lerp(Start.mainThrottle, Target.mainThrottle, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.wheelSteer = Lerp(Start.wheelSteer, Target.wheelSteer, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.wheelSteerTrim = Lerp(Start.wheelSteerTrim, Target.wheelSteerTrim, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.wheelThrottle = Lerp(Start.wheelThrottle, Target.wheelThrottle, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.wheelThrottleTrim = Lerp(Start.wheelThrottleTrim, Target.wheelThrottleTrim, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.gearDown = Lerp(Start.gearDown, Target.gearDown, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.gearUp = Lerp(Start.gearUp, Target.gearUp, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.headlight = Lerp(Start.headlight, Target.headlight, Mathf.Clamp01(_lerpPercentage));
            _currentFlightControlState.killRot = Lerp(Start.killRot, Target.killRot, Mathf.Clamp01(_lerpPercentage));

            if(InterpolationDuration > 0)
                _lerpPercentage += Time.fixedDeltaTime / InterpolationDuration;

            return _currentFlightControlState;
        }

        private static float Lerp(float v0, float v1, float t)
        {
            return (1 - t) * v0 + t * v1;
        }

        private static bool Lerp(bool v0, bool v1, float t)
        {
            return t < 0.5 ? v0 : v1;
        }
    }
}

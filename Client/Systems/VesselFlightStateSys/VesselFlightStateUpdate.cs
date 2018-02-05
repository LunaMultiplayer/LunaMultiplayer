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

        public bool InterpolationFinished => _lerpPercentage >= 1;
        public float InterpolationDuration => (float)TimeSpan.FromTicks(EndTimeStamp - StartTimeStamp).TotalSeconds;


        private readonly FlightCtrlState _currentFlightControlState = new FlightCtrlState();
        
        public bool CanInterpolate => Start != null && Target != null;

        private float _lerpPercentage;

        public void SetTarget(VesselFlightStateMsgData msgData)
        {
            //If we are in the middle of an interpolation let it finish
            if (CanInterpolate && !InterpolationFinished) return;

            if (Start == null)
            {
                Start = GetFlightCtrlStateFromMsg(msgData);
                StartTimeStamp = msgData.TimeStamp;

                return;
            }

            if (Target == null)
            {
                Target = GetFlightCtrlStateFromMsg(msgData);
                EndTimeStamp = msgData.TimeStamp;

                return;
            }

            //We've finished the interpolation so...
            _lerpPercentage = 0;

            Start.CopyFrom(Target);
            StartTimeStamp = EndTimeStamp;

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

        public FlightCtrlState GetInterpolatedValue()
        {
            _currentFlightControlState.X = Lerp(Start.X, Target.X, _lerpPercentage);
            _currentFlightControlState.Y = Lerp(Start.Y, Target.Y, _lerpPercentage);
            _currentFlightControlState.Z = Lerp(Start.Z, Target.Z, _lerpPercentage);
            _currentFlightControlState.pitch = Lerp(Start.pitch, Target.pitch, _lerpPercentage);
            _currentFlightControlState.pitchTrim = Lerp(Start.pitchTrim, Target.pitchTrim, _lerpPercentage);
            _currentFlightControlState.roll = Lerp(Start.roll, Target.roll, _lerpPercentage);
            _currentFlightControlState.rollTrim = Lerp(Start.rollTrim, Target.rollTrim, _lerpPercentage);
            _currentFlightControlState.yaw = Lerp(Start.yaw, Target.yaw, _lerpPercentage);
            _currentFlightControlState.yawTrim = Lerp(Start.yawTrim, Target.yawTrim, _lerpPercentage);
            _currentFlightControlState.mainThrottle = Lerp(Start.mainThrottle, Target.mainThrottle, _lerpPercentage);
            _currentFlightControlState.wheelSteer = Lerp(Start.wheelSteer, Target.wheelSteer, _lerpPercentage);
            _currentFlightControlState.wheelSteerTrim = Lerp(Start.wheelSteerTrim, Target.wheelSteerTrim, _lerpPercentage);
            _currentFlightControlState.wheelThrottle = Lerp(Start.wheelThrottle, Target.wheelThrottle, _lerpPercentage);
            _currentFlightControlState.wheelThrottleTrim = Lerp(Start.wheelThrottleTrim, Target.wheelThrottleTrim, _lerpPercentage);
            _currentFlightControlState.gearDown = Lerp(Start.gearDown, Target.gearDown, _lerpPercentage);
            _currentFlightControlState.gearUp = Lerp(Start.gearUp, Target.gearUp, _lerpPercentage);
            _currentFlightControlState.headlight = Lerp(Start.headlight, Target.headlight, _lerpPercentage);
            _currentFlightControlState.killRot = Lerp(Start.killRot, Target.killRot, _lerpPercentage);

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

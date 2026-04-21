using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageSender : SubSystem<VesselFlightStateSystem>, IMessageSender
    {
        private const double ForceResyncIntervalMs = 500;
        private static readonly FlightCtrlState LastSentFlightState = new FlightCtrlState();
        private static Guid _lastSentVesselId;
        private static DateTime _lastFlightStateSentAt = DateTime.MinValue;

        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendCurrentFlightState()
        {
            var flightState = new FlightCtrlState();
            flightState.CopyFrom(FlightGlobals.ActiveVessel.ctrlState);

            var vesselId = FlightGlobals.ActiveVessel.id;
            var forceResync = vesselId != _lastSentVesselId || (DateTime.UtcNow - _lastFlightStateSentAt).TotalMilliseconds >= ForceResyncIntervalMs;
            if (!forceResync && FlightStatesAreEquivalent(flightState, LastSentFlightState))
                return;

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselFlightStateMsgData>();
            msgData.PingSec = NetworkStatistics.PingSec;

            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.SubspaceId = WarpSystem.Singleton.CurrentSubspace;

            msgData.VesselId = vesselId;
            msgData.GearDown = flightState.gearDown;
            msgData.GearUp = flightState.gearUp;
            msgData.Headlight = flightState.headlight;
            msgData.KillRot = flightState.killRot;
            msgData.MainThrottle = flightState.mainThrottle;
            msgData.Pitch = flightState.pitch;
            msgData.PitchTrim = flightState.pitchTrim;
            msgData.Roll = flightState.roll;
            msgData.RollTrim = flightState.rollTrim;
            msgData.WheelSteer = flightState.wheelSteer;
            msgData.WheelSteerTrim = flightState.wheelSteerTrim;
            msgData.WheelThrottle = flightState.wheelThrottle;
            msgData.WheelThrottleTrim = flightState.wheelThrottleTrim;
            msgData.X = flightState.X;
            msgData.Y = flightState.Y;
            msgData.Yaw = flightState.yaw;
            msgData.YawTrim = flightState.yawTrim;
            msgData.Z = flightState.Z;

            SendMessage(msgData);

            LastSentFlightState.CopyFrom(flightState);
            _lastSentVesselId = vesselId;
            _lastFlightStateSentAt = DateTime.UtcNow;
        }

        private static bool FlightStatesAreEquivalent(FlightCtrlState current, FlightCtrlState previous)
        {
            return ApproximatelyEqual(current.mainThrottle, previous.mainThrottle)
                && ApproximatelyEqual(current.wheelThrottle, previous.wheelThrottle)
                && ApproximatelyEqual(current.wheelThrottleTrim, previous.wheelThrottleTrim)
                && ApproximatelyEqual(current.X, previous.X)
                && ApproximatelyEqual(current.Y, previous.Y)
                && ApproximatelyEqual(current.Z, previous.Z)
                && current.killRot == previous.killRot
                && current.gearUp == previous.gearUp
                && current.gearDown == previous.gearDown
                && current.headlight == previous.headlight
                && ApproximatelyEqual(current.pitch, previous.pitch)
                && ApproximatelyEqual(current.roll, previous.roll)
                && ApproximatelyEqual(current.yaw, previous.yaw)
                && ApproximatelyEqual(current.pitchTrim, previous.pitchTrim)
                && ApproximatelyEqual(current.rollTrim, previous.rollTrim)
                && ApproximatelyEqual(current.yawTrim, previous.yawTrim)
                && ApproximatelyEqual(current.wheelSteer, previous.wheelSteer)
                && ApproximatelyEqual(current.wheelSteerTrim, previous.wheelSteerTrim);
        }

        private static bool ApproximatelyEqual(float left, float right)
        {
            return Math.Abs(left - right) < 0.001f;
        }
    }
}

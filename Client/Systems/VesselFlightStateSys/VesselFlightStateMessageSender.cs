using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageSender : SubSystem<VesselFlightStateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            TaskFactory.StartNew(() => NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg)));
        }

        public void SendCurrentFlightState()
        {
            var flightState = new FlightCtrlState();
            flightState.CopyFrom(FlightGlobals.ActiveVessel.ctrlState);

            var msg = new VesselFlightStateMsgData
            {
                VesselId = FlightGlobals.ActiveVessel.id,
                GearDown = flightState.gearDown,
                GearUp = flightState.gearUp,
                Headlight = flightState.headlight,
                KillRot = flightState.killRot,
                MainThrottle = flightState.mainThrottle,
                Pitch = flightState.pitch,
                PitchTrim = flightState.pitchTrim,
                Roll = flightState.roll,
                RollTrim = flightState.rollTrim,
                WheelSteer = flightState.wheelSteer,
                WheelSteerTrim = flightState.wheelSteerTrim,
                WheelThrottle = flightState.wheelThrottle,
                WheelThrottleTrim = flightState.wheelThrottleTrim,
                X = flightState.X,
                Y = flightState.Y,
                Yaw = flightState.yaw,
                YawTrim = flightState.yawTrim,
                Z = flightState.Z
            };

            SendMessage(msg);
        }
    }
}

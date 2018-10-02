using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageSender : SubSystem<VesselFlightStateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendCurrentFlightState()
        {
            var flightState = new FlightCtrlState();
            flightState.CopyFrom(FlightGlobals.ActiveVessel.ctrlState);

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<VesselFlightStateMsgData>();
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            msgData.SubspaceId = WarpSystem.Singleton.CurrentSubspace;

            msgData.VesselId = FlightGlobals.ActiveVessel.id;
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
        }
    }
}

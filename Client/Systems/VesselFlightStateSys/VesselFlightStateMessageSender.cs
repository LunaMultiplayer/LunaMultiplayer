using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselFlightStateSys
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
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
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

            VesselsProtoStore.UpdateVesselProtoFlightState(msgData);
            System.UpdateFlightStateInProtoVessel(FlightGlobals.ActiveVessel.protoVessel, msgData);
        }
    }
}

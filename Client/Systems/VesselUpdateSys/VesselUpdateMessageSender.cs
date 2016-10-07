using System;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.Network;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageSender:SubSystem<VesselUpdateSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSystem.Singleton.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselUpdate(VesselUpdate update)
        {
            var msg = new VesselUpdateMsgData
            {
                SentTime = DateTime.UtcNow.Ticks,
                PlanetTime = Planetarium.GetUniversalTime(),
                VesselId = update.VesselId,
                Acceleration = update.Acceleration,
                ActiongroupControls = update.ActionGrpControls,
                AngularVelocity = update.AngularVel,
                BodyName = update.BodyName,
                GearDown = update.FlightState.gearDown,
                GearUp = update.FlightState.gearUp,
                Headlight = update.FlightState.headlight,
                IsSurfaceUpdate = update.IsSurfaceUpdate,
                KillRot = update.FlightState.killRot,
                MainThrottle = update.FlightState.mainThrottle,
                Orbit = update.Orbit,
                Pitch = update.FlightState.pitch,
                PitchTrim = update.FlightState.pitchTrim,
                Position = update.Position,
                Roll = update.FlightState.roll,
                RollTrim = update.FlightState.rollTrim,
                Rotation = update.Rotation,
                Velocity = update.Velocity,
                WheelSteer = update.FlightState.wheelSteer,
                WheelSteerTrim = update.FlightState.wheelSteerTrim,
                WheelThrottle = update.FlightState.wheelThrottle,
                WheelThrottleTrim = update.FlightState.wheelThrottleTrim,
                X = update.FlightState.X,
                Y = update.FlightState.Y,
                Yaw = update.FlightState.yaw,
                YawTrim = update.FlightState.yawTrim,
                Z = update.FlightState.Z
            };

            SendMessage(msg);
        }
    }
}

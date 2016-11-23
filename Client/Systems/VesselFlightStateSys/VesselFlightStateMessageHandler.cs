using System.Collections.Concurrent;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageHandler : SubSystem<VesselFlightStateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselFlightStateMsgData;
            if (msgData == null) return;

            if (System.FlightStatesDictionary.ContainsKey(msgData.VesselId))
            {
                var flightState = new FlightCtrlState
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

                System.FlightStatesDictionary[msgData.VesselId] = flightState;
            }
        }
    }
}

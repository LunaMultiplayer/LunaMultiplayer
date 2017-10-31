using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System.Collections.Concurrent;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateMessageHandler : SubSystem<VesselFlightStateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            if (!(messageData is VesselFlightStateMsgData msgData)) return;

            if (System.FlightStatesDictionary.TryGetValue(msgData.VesselId, out var existingFlightState))
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

                System.FlightStatesDictionary.TryUpdate(msgData.VesselId, flightState, existingFlightState);
            }
        }
    }
}

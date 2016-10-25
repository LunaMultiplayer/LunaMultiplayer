using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Systems.VesselLockSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    public class VesselUpdateMessageHandler : SubSystem<VesselUpdateSystem>, IMessageHandler
    {
        public ConcurrentQueue<IMessageData> IncomingMessages { get; set; } = new ConcurrentQueue<IMessageData>();

        public void HandleMessage(IMessageData messageData)
        {
            var msgData = messageData as VesselUpdateMsgData;

            if (msgData == null || !System.UpdateSystemReady || UpdateIsForOwnVessel(msgData.VesselId))
            {
                return;
            }

            var update = new VesselUpdate
            {
                Id = Guid.NewGuid(),
                ReceiveTime = Time.fixedTime,
                Stage = msgData.Stage,
                SentTime = msgData.SentTime,
                ActiveEngines = msgData.ActiveEngines,
                StoppedEngines = msgData.StoppedEngines,
                Decouplers = msgData.Decouplers,
                AnchoredDecouplers = msgData.AnchoredDecouplers,
                Clamps = msgData.Clamps,
                Docks = msgData.Docks,
                VesselId = msgData.VesselId,
                BodyName = msgData.BodyName,
                Rotation = msgData.Rotation,
                FlightState = new FlightCtrlState
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
                },
                ActionGrpControls = msgData.ActiongroupControls,
                IsSurfaceUpdate = msgData.IsSurfaceUpdate
            };

            if (update.IsSurfaceUpdate)
            {
                update.Position = msgData.Position;
                update.Velocity = msgData.Velocity;
                update.Acceleration = msgData.Acceleration;
            }
            else
            {
                update.Orbit = msgData.Orbit;
            }

            if (!System.ReceivedUpdates.ContainsKey(update.VesselId))
            {
                System.ReceivedUpdates.Add(update.VesselId, new Queue<VesselUpdate>());
            }

            System.ReceivedUpdates[update.VesselId].Enqueue(update);
        }

        private bool UpdateIsForOwnVessel(Guid vesselId)
        {
            //Ignore updates to our own vessel if we aren't spectating
            return !VesselLockSystem.Singleton.IsSpectating &&
                   (FlightGlobals.ActiveVessel != null) &&
                   (FlightGlobals.ActiveVessel.id == vesselId);
        }
    }
}

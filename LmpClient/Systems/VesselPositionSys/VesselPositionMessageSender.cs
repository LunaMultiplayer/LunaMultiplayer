using LmpClient.Base;
using LmpClient.Base.Interface;
using LmpClient.Extensions;
using LmpClient.Network;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpClient.Utilities;
using LmpCommon.Message.Client;
using LmpCommon.Message.Data.Vessel;
using LmpCommon.Message.Interface;
using System;
using UnityEngine;

namespace LmpClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageSender : SubSystem<VesselPositionSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        /// <summary>
        /// Sends a vessel position update
        /// </summary>
        /// <param name="vessel">Vessel to send the position</param>
        /// <param name="doOrbitDriverReadyCheck">Set it to true if you want to check if the driver is ready.
        /// Avoid checking it unless is really needed as it uses reflection that's slow</param>
        public void SendVesselPositionUpdate(Vessel vessel, bool doOrbitDriverReadyCheck = false)
        {
            if (vessel == null) return;

            if (doOrbitDriverReadyCheck && !vessel.orbitDriver.Ready())
            {
                //Orbit driver is not ready so wait max 10 frames until it's ready
                CoroutineUtil.StartConditionRoutine("SendVesselPositionUpdate",
                    () => SendVesselPositionUpdate(vessel),
                    () => vessel.orbitDriver.Ready(), 10);

            }
            else
            {
                var msg = CreateMessageFromVessel(vessel);
                if (msg == null) return;

                SendMessage(msg);
            }
        }

        public static VesselPositionMsgData CreateMessageFromVessel(Vessel vessel)
        {
            if (!OrbitParametersAreOk(vessel)) return null;

            var msgData = MessageFactory.CreateNewMessageData<VesselPositionMsgData>();
            msgData.PingSec = NetworkStatistics.PingSec;
            msgData.SubspaceId = WarpSystem.Singleton.CurrentSubspace;
            msgData.GameTime = TimeSyncSystem.UniversalTime;
            try
            {
                msgData.VesselId = vessel.id;
                msgData.BodyIndex = vessel.mainBody.flightGlobalsIndex;
                msgData.Landed = vessel.Landed;
                msgData.Splashed = vessel.Splashed;

                SetSrfRelRotation(vessel, msgData);
                SetLatLonAlt(vessel, msgData);
                SetVelocityVector(vessel, msgData);
                SetNormalVector(vessel, msgData);
                SetOrbit(vessel, msgData);

                msgData.HeightFromTerrain = vessel.heightFromTerrain;

                if (MainSystem.BodiesGees.TryGetValue(vessel.mainBody, out var bodyGee))
                    msgData.HackingGravity = Math.Abs(bodyGee - vessel.mainBody.GeeASL) > 0.0001;
                msgData.HackingGravity = false;

                return msgData;
            }
            catch (Exception e)
            {
                LunaLog.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
            }

            return null;
        }

        #region Set message values

        private static void SetOrbit(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.Orbit[0] = vessel.orbit.inclination;
            msgData.Orbit[1] = vessel.orbit.eccentricity;
            msgData.Orbit[2] = vessel.orbit.semiMajorAxis;
            msgData.Orbit[3] = vessel.orbit.LAN;
            msgData.Orbit[4] = vessel.orbit.argumentOfPeriapsis;
            msgData.Orbit[5] = vessel.orbit.meanAnomalyAtEpoch;
            msgData.Orbit[6] = vessel.orbit.epoch;
            msgData.Orbit[7] = vessel.orbit.referenceBody.flightGlobalsIndex;
        }

        private static void SetVelocityVector(Vessel vessel, VesselPositionMsgData msgData)
        {
            var velVector = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
            msgData.VelocityVector[0] = velVector.x;
            msgData.VelocityVector[1] = velVector.y;
            msgData.VelocityVector[2] = velVector.z;
        }

        private static void SetNormalVector(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.NormalVector[0] = vessel.terrainNormal.x;
            msgData.NormalVector[1] = vessel.terrainNormal.y;
            msgData.NormalVector[2] = vessel.terrainNormal.z;
        }

        private static void SetLatLonAlt(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.LatLonAlt[0] = vessel.latitude;
            msgData.LatLonAlt[1] = vessel.longitude;
            msgData.LatLonAlt[2] = vessel.altitude;
        }

        private static void SetSrfRelRotation(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.SrfRelRotation[0] = vessel.srfRelRotation.x;
            msgData.SrfRelRotation[1] = vessel.srfRelRotation.y;
            msgData.SrfRelRotation[2] = vessel.srfRelRotation.z;
            msgData.SrfRelRotation[3] = vessel.srfRelRotation.w;
        }

        #endregion

        /// <summary>
        /// Checks if the vessel contains NaN in any orbit parameter
        /// </summary>
        private static bool OrbitParametersAreOk(Vessel vessel)
        {
            var orbitParamsAreNan = double.IsNaN(vessel.orbit.inclination) ||
                                    double.IsNaN(vessel.orbit.eccentricity) ||
                                    double.IsNaN(vessel.orbit.semiMajorAxis) ||
                                    double.IsNaN(vessel.orbit.LAN) ||
                                    double.IsNaN(vessel.orbit.argumentOfPeriapsis) ||
                                    double.IsNaN(vessel.orbit.meanAnomalyAtEpoch) ||
                                    double.IsNaN(vessel.orbit.epoch) ||
                                    double.IsNaN(vessel.orbit.referenceBody.flightGlobalsIndex);

            return !orbitParamsAreNan;
        }
    }
}

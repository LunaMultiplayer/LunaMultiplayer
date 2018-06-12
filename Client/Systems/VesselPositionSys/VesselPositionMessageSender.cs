using LunaClient.Base;
using LunaClient.Base.Interface;
using LunaClient.Network;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.VesselStore;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionMessageSender : SubSystem<VesselPositionSystem>, IMessageSender
    {
        public void SendMessage(IMessageData msg)
        {
            NetworkSender.QueueOutgoingMessage(MessageFactory.CreateNew<VesselCliMsg>(msg));
        }

        public void SendVesselPositionUpdate(Vessel vessel)
        {
            if (vessel == null) return;

            var msg = CreateMessageFromVessel(vessel);
            if (msg == null) return;

            //Update our own values in the store aswell as otherwise if we leave the vessel it can still be in the safety bubble
            VesselsProtoStore.UpdateVesselProtoPosition(msg);
            UpdateOwnVesselProtoFields(vessel, msg.BodyIndex);

            SendMessage(msg);
        }

        private void UpdateOwnVesselProtoFields(Vessel vessel, int bodyIndex)
        {
            if (vessel.protoVessel == null) return;

            if (vessel.protoVessel.orbitSnapShot != null)
            {
                vessel.protoVessel.orbitSnapShot.semiMajorAxis = vessel.orbit.semiMajorAxis;
                vessel.protoVessel.orbitSnapShot.eccentricity = vessel.orbit.eccentricity;
                vessel.protoVessel.orbitSnapShot.inclination = vessel.orbit.inclination;
                vessel.protoVessel.orbitSnapShot.argOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
                vessel.protoVessel.orbitSnapShot.LAN = vessel.orbit.LAN;
                vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
                vessel.protoVessel.orbitSnapShot.epoch = vessel.orbit.epoch;
                vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = bodyIndex;
            }

            vessel.protoVessel.latitude = vessel.latitude;
            vessel.protoVessel.longitude = vessel.longitude;
            vessel.protoVessel.altitude = vessel.altitude;
            vessel.protoVessel.height = vessel.heightFromTerrain;
            vessel.protoVessel.normal = vessel.terrainNormal;
            vessel.protoVessel.rotation = vessel.srfRelRotation;
        }


        public static VesselPositionMsgData CreateMessageFromVessel(Vessel vessel)
        {
            if (!OrbitParametersAreOk(vessel)) return null;

            var msgData = MessageFactory.CreateNewMessageData<VesselPositionMsgData>();
            msgData.SubspaceId = WarpSystem.Singleton.CurrentSubspace;
            msgData.GameTime = TimeSyncerSystem.UniversalTime;
            try
            {
                msgData.VesselId = vessel.id;
                msgData.BodyIndex = vessel.mainBody.flightGlobalsIndex;
                msgData.Landed = vessel.Landed;
                msgData.Splashed = vessel.Splashed;

                SetSrfRelRotation(vessel, msgData);
                SetVelocity(vessel, msgData);
                SetLatLonAlt(vessel, msgData);
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

        private static void SetVelocity(Vessel vessel, VesselPositionMsgData msgData)
        {
            Vector3d srfVel = UnityEngine.Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
            msgData.Velocity[0] = srfVel.x;
            msgData.Velocity[1] = srfVel.y;
            msgData.Velocity[2] = srfVel.z;
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

using LunaClient.Base;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using LunaCommon.Time;
using System;

namespace LunaClient.Systems.VesselPositionSys
{
    public class MessageToPositionTransfer
    {
        public static VesselPositionUpdate CreateFromMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return null;

            var upd = new VesselPositionUpdate
            {
                VesselId = msgData.VesselId,
                BodyIndex = msgData.BodyIndex,
                HeightFromTerrain = msgData.HeightFromTerrain,
                TimeStamp = msgData.TimeStamp
            };

            Array.Copy(msgData.SrfRelRotation, upd.SrfRelRotation, 4);
            Array.Copy(msgData.TransformPosition, upd.TransformPosition, 3);
            Array.Copy(msgData.Velocity, upd.Velocity, 3);
            Array.Copy(msgData.LatLonAlt, upd.LatLonAlt, 3);
            Array.Copy(msgData.Com, upd.Com, 3);
            Array.Copy(msgData.NormalVector, upd.NormalVector, 3);
            Array.Copy(msgData.Orbit, upd.Orbit, 8);

            return upd;
        }

        public static VesselPositionUpdate UpdateFromMessage(IServerMessageBase msg, VesselPositionUpdate update)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return null;

            update.VesselId = msgData.VesselId;
            update.BodyIndex = msgData.BodyIndex;
            update.HeightFromTerrain = msgData.HeightFromTerrain;
            update.TimeStamp = msgData.TimeStamp;

            Array.Copy(msgData.SrfRelRotation, update.SrfRelRotation, 4);
            Array.Copy(msgData.TransformPosition, update.TransformPosition, 3);
            Array.Copy(msgData.Velocity, update.Velocity, 3);
            Array.Copy(msgData.LatLonAlt, update.LatLonAlt, 3);
            Array.Copy(msgData.Com, update.Com, 3);
            Array.Copy(msgData.NormalVector, update.NormalVector, 3);
            Array.Copy(msgData.Orbit, update.Orbit, 8);

            return update;
        }

        public static VesselPositionUpdate UpdateFromUpdate(VesselPositionUpdate update, VesselPositionUpdate updateToUpdate)
        {
            updateToUpdate.VesselId = update.VesselId;
            updateToUpdate.BodyIndex = update.BodyIndex;
            updateToUpdate.HeightFromTerrain = update.HeightFromTerrain;
            updateToUpdate.TimeStamp = update.TimeStamp;

            Array.Copy(update.SrfRelRotation, updateToUpdate.SrfRelRotation, 4);
            Array.Copy(update.TransformPosition, updateToUpdate.TransformPosition, 3);
            Array.Copy(update.Velocity, updateToUpdate.Velocity, 3);
            Array.Copy(update.LatLonAlt, updateToUpdate.LatLonAlt, 3);
            Array.Copy(update.Com, updateToUpdate.Com, 3);
            Array.Copy(update.NormalVector, updateToUpdate.NormalVector, 3);
            Array.Copy(update.Orbit, updateToUpdate.Orbit, 8);

            return update;
        }
        
        public static VesselPositionMsgData CreateMessageFromVessel(Vessel vessel)
        {
            var msgData = SystemBase.MessageFactory.CreateNewMessageData<VesselPositionMsgData>();
            try
            {
                msgData.VesselId = vessel.id;
                msgData.BodyIndex = vessel.mainBody.flightGlobalsIndex;

                SetSrfRelRotation(vessel, msgData);
                SetTransformPosition(vessel, msgData);
                SetVelocity(vessel, msgData);
                SetLatLonAlt(vessel, msgData);
                GetCom(vessel, msgData);
                SetNormalVector(vessel, msgData);
                SetOrbit(vessel, msgData);

                msgData.HeightFromTerrain = vessel.heightFromTerrain;
                msgData.TimeStamp = LunaTime.UtcNow.Ticks;
                
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

        private static void GetCom(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.Com[0] = vessel.CoM.x;
            msgData.Com[1] = vessel.CoM.y;
            msgData.Com[2] = vessel.CoM.z;
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

        private static void SetTransformPosition(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.TransformPosition[0] = vessel.ReferenceTransform.position.x;
            msgData.TransformPosition[1] = vessel.ReferenceTransform.position.y;
            msgData.TransformPosition[2] = vessel.ReferenceTransform.position.z;
        }

        private static void SetSrfRelRotation(Vessel vessel, VesselPositionMsgData msgData)
        {
            msgData.SrfRelRotation[0] = vessel.srfRelRotation.x;
            msgData.SrfRelRotation[1] = vessel.srfRelRotation.y;
            msgData.SrfRelRotation[2] = vessel.srfRelRotation.z;
            msgData.SrfRelRotation[3] = vessel.srfRelRotation.w;
        }

        #endregion
    }
}

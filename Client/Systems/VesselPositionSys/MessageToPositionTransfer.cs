using LunaClient.Base;
using LunaCommon.Message.Client;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;

namespace LunaClient.Systems.VesselPositionSys
{
    public class MessageToPositionTransfer
    {
        public static VesselPositionUpdate CreateFromMessage(IServerMessageBase msg)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return null;

            return new VesselPositionUpdate
            {
                VesselId = msgData.VesselId,
                BodyName = msgData.BodyName,
                TransformRotation = msgData.TransformRotation,
                TransformPosition = msgData.TransformPosition,
                Velocity = msgData.Velocity,
                LatLonAlt = msgData.LatLonAlt,
                Landed = msgData.Landed,
                Splashed = msgData.Splashed,
                Com = msgData.Com,
                NormalVector = msgData.NormalVector,
                Orbit = msgData.Orbit,
                SentTime = msgData.SentTime
            };
        }

        public static VesselPositionUpdate UpdateFromMessage(IServerMessageBase msg, VesselPositionUpdate update)
        {
            if (!(msg.Data is VesselPositionMsgData msgData)) return null;

            update.VesselId = msgData.VesselId;
            update.BodyName = msgData.BodyName;
            update.TransformRotation = msgData.TransformRotation;
            update.TransformPosition = msgData.TransformPosition;
            update.Velocity = msgData.Velocity;
            update.LatLonAlt = msgData.LatLonAlt;
            update.Landed = msgData.Landed;
            update.Splashed = msgData.Splashed;
            update.Com = msgData.Com;
            update.NormalVector = msgData.NormalVector;
            update.Orbit = msgData.Orbit;
            update.SentTime = msgData.SentTime;

            return update;
        }

        public static VesselPositionUpdate UpdateFromUpdate(VesselPositionUpdate update, VesselPositionUpdate updateToUpdate)
        {
            updateToUpdate.VesselId = update.VesselId;
            updateToUpdate.BodyName = update.BodyName;
            updateToUpdate.TransformRotation = update.TransformRotation;
            updateToUpdate.TransformPosition = update.TransformPosition;
            updateToUpdate.Velocity = update.Velocity;
            updateToUpdate.LatLonAlt = update.LatLonAlt;
            updateToUpdate.Landed = update.Landed;
            updateToUpdate.Splashed = update.Splashed;
            updateToUpdate.Com = update.Com;
            updateToUpdate.NormalVector = update.NormalVector;
            updateToUpdate.Orbit = update.Orbit;
            updateToUpdate.SentTime = update.SentTime;

            return update;
        }

        public static VesselCliMsg CreateMessageFromVessel(Vessel vessel)
        {
            var msgData = SystemBase.MessageFactory.CreateNewMessageData<VesselPositionMsgData>();
            var msg = SystemBase.MessageFactory.CreateNew<VesselCliMsg>(msgData);
            try
            {
                msgData.VesselId = vessel.id;
                msgData.BodyName = vessel.mainBody.bodyName;

                msgData.TransformRotation = new[]
                {
                    vessel.ReferenceTransform.rotation.x,
                    vessel.ReferenceTransform.rotation.y,
                    vessel.ReferenceTransform.rotation.z,
                    vessel.ReferenceTransform.rotation.w
                };
                msgData.TransformPosition = new[]
                {
                    (double)vessel.ReferenceTransform.position.x,
                    (double)vessel.ReferenceTransform.position.y,
                    (double)vessel.ReferenceTransform.position.z
                };
                msgData.Velocity = new[]
                {
                    (double)vessel.rb_velocity.x,
                    (double)vessel.rb_velocity.y,
                    (double)vessel.rb_velocity.z,
                };
                msgData.LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude,
                };
                msgData.Com = new[]
                {
                    (double)vessel.CoM.x,
                    (double)vessel.CoM.y,
                    (double)vessel.CoM.z,
                };
                msgData.NormalVector = new[]
                {
                    (double)vessel.terrainNormal.x,
                    (double)vessel.terrainNormal.y,
                    (double)vessel.terrainNormal.z,
                };
                msgData.Orbit = new[]
                {
                    vessel.orbit.inclination,
                    vessel.orbit.eccentricity,
                    vessel.orbit.semiMajorAxis,
                    vessel.orbit.LAN,
                    vessel.orbit.argumentOfPeriapsis,
                    vessel.orbit.meanAnomalyAtEpoch,
                    vessel.orbit.epoch,
                    vessel.orbit.referenceBody.flightGlobalsIndex
                };
                msgData.Landed = vessel.Landed;
                msgData.Splashed = vessel.Splashed;

                return SystemBase.MessageFactory.CreateNew<VesselCliMsg>(msgData);
            }
            catch (Exception e)
            {
                msg.Recycle();
                LunaLog.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
            }

            return null;
        }
    }
}

using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

// ReSharper disable All

namespace LunaClient.Systems.VesselPositionAltSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionAltUpdate
    {
        #region Fields

        public float SentTime { get; set; }
        public Vessel Vessel { get; set; }
        public CelestialBody Body { get; set; }
        public Guid Id { get; set; }
        public double PlanetTime { get; set; }

        #region Vessel position information fields

        public Guid VesselId { get; set; }
        public string BodyName { get; set; }
        public float[] TransformRotation { get; set; }
        public double[] TransformPosition { get; set; }

        public Quaternion Rotation => new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);

        public Vector3 TransformPos => new Vector3d(TransformPosition[0], TransformPosition[1], TransformPosition[2]);

        #region Orbit field

        public double[] Orbit { get; set; }

        #endregion

        #region Surface fields

        public double[] LatLonAlt { get; set; }

        private Vector3d _position = Vector3d.zero;
        private Vector3d Position
        {
            get
            {
                if (_position == Vector3d.zero && Body != null)
                {
                    _position = Body.GetWorldSurfacePosition(LatLonAlt[0], LatLonAlt[1], LatLonAlt[2]);
                }
                return _position;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Constructors/Creation

        public VesselPositionMsgData GetParsedMessage()
        {
            return new VesselPositionMsgData
            {
                GameSentTime = Time.time,
                PlanetTime = PlanetTime,
                VesselId = VesselId,
                BodyName = BodyName,
                Orbit = Orbit,
                LatLonAlt = LatLonAlt,
                TransformPosition = TransformPosition,
                TransformRotation = TransformRotation,
            };
        }

        public VesselPositionAltUpdate(VesselPositionMsgData msgData)
        {
            Id = Guid.NewGuid();
            PlanetTime = msgData.PlanetTime;
            SentTime = msgData.GameSentTime;
            VesselId = msgData.VesselId;
            BodyName = msgData.BodyName;
            TransformRotation = msgData.TransformRotation;
            TransformPosition = msgData.TransformPosition;
            LatLonAlt = msgData.LatLonAlt;
            Orbit = msgData.Orbit;
        }

        public VesselPositionAltUpdate(Vessel vessel)
        {
            try
            {
                VesselId = vessel.id;
                PlanetTime = Planetarium.GetUniversalTime();
                BodyName = vessel.mainBody.bodyName;
                ////Update the vessel's orbital information from the current position of the rigidbodies
                //vessel.orbitDriver.TrackRigidbody(vessel.mainBody, 0);
                //vessel.UpdatePosVel();

                TransformRotation = new[]
                {
                    vessel.vesselTransform.rotation.x,
                    vessel.vesselTransform.rotation.y,
                    vessel.vesselTransform.rotation.z,
                    vessel.vesselTransform.rotation.w
                };
                TransformPosition = new[]
                {
                    (double)vessel.vesselTransform.position.x,
                    (double)vessel.vesselTransform.position.y,
                    (double)vessel.vesselTransform.position.z
                };
                LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude
                };
                Orbit = new[]
                {
                    vessel.orbit.inclination,
                    vessel.orbit.eccentricity,
                    vessel.orbit.semiMajorAxis,
                    vessel.orbit.LAN,
                    vessel.orbit.argumentOfPeriapsis,
                    vessel.orbit.meanAnomalyAtEpoch,
                    vessel.orbit.epoch
                };
            }
            catch (Exception e)
            {
                LunaLog.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
            }
        }

        public virtual VesselPositionAltUpdate Clone()
        {
            return MemberwiseClone() as VesselPositionAltUpdate;
        }

        #endregion

        #region Main method

        /// <summary>
        /// Apply the vessel update.  Run on FixedUpdate as we're updating physics, therefore it cannot be run in update()
        /// </summary>
        /// <returns></returns>
        public void ApplyVesselUpdate()
        {
            if (!SetBodyAndVessel()) return;

            Vessel.checkSplashed();
            Vessel.checkLanded();

            if (!Vessel.LandedOrSplashed)
                ApplyOrbitData();

            Vessel.vesselTransform.rotation = Rotation;
            if (!Vessel.loaded)
            {
                Vessel.latitude = LatLonAlt[0];
                Vessel.longitude = LatLonAlt[1];
                Vessel.altitude = LatLonAlt[2];
                return;
            }

            if (Vessel.packed)
            {
                Vessel.transform.position = TransformPos;
                Vessel.SetRotation(Rotation, true);
                Vessel.srfRelRotation = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * Vessel.vesselTransform.rotation;
                //Vessel.precalc.worldSurfaceRot = Vessel.mainBody.bodyTransform.rotation * Vessel.srfRelRotation;
                Vessel.mainBody.GetLatLonAlt(TransformPos, out Vessel.latitude, out Vessel.longitude, out Vessel.altitude);
            }
            else
            {
                Vessel.vesselTransform.position = Position;
                Vessel.SetRotation(Rotation, true);
                Vessel.SetPosition(Position, false);
                Vessel.latitude = LatLonAlt[0];
                Vessel.longitude = LatLonAlt[1];
                Vessel.altitude = LatLonAlt[2];

                foreach (var part in Vessel.parts)
                    part.ResumeVelocity();
            }
        }

        private bool SetBodyAndVessel()
        {
            if (Body == null)
            {
                Body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
                if (Body == null) return false;
            }
            if (Vessel == null)
            {
                Vessel = FlightGlobals.Vessels.Find(v => v.id == VesselId);
                if (Vessel == null) return false;
            }
            return true;
        }

        private void ApplyOrbitData()
        {
            Vessel.orbit.inclination = Orbit[0];
            Vessel.orbit.eccentricity = Orbit[1];
            Vessel.orbit.semiMajorAxis = Orbit[2];
            Vessel.orbit.LAN = Orbit[3];
            Vessel.orbit.argumentOfPeriapsis = Orbit[4];
            Vessel.orbit.meanAnomalyAtEpoch = Orbit[5];
            Vessel.orbit.epoch = Orbit[6];
            Vessel.orbit.referenceBody = Body;
        }

        private void UpdateProtoVesselPosition()
        {
            //Update the protovessel, so if the vessel gets saved out, it's at the right spot
            //Vessel.protoVessel.latitude = LatLonAlt[0];
            //Vessel.protoVessel.longitude = LatLonAlt[1];
            //Vessel.protoVessel.altitude = LatLonAlt[2];
            //Vessel.protoVessel.position = Vessel.vesselTransform.position;
            //Vessel.precalc.worldSurfacePos = Position;
        }

        #endregion
    }
}
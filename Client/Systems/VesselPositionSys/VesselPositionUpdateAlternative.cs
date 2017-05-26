using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys
{
    public class VesselPositionUpdateAlternative
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

        #region Orbit field

        public double[] Orbit { get; set; }

        #endregion

        #region Surface fields

        public double[] LatLonAlt { get; set; }
        public double[] WorldPosition { get; set; }
        public double[] OrbitPosition { get; set; }
        public double[] Velocity { get; set; }
        public double[] OrbitVelocity { get; set; }
        public double[] Acceleration { get; set; }

        private Vector3d _surfacePosition = Vector3d.zero;
        private Vector3d SurfacePosition
        {
            get
            {
                if (_surfacePosition == Vector3d.zero && Body != null)
                    _surfacePosition = Body.GetWorldSurfacePosition(LatLonAlt[0], LatLonAlt[1], LatLonAlt[2]);
                return _surfacePosition;
            }
        }

        #endregion

        #endregion

        #region Private fields

        private double PlanetariumDifference { get; set; }

        #endregion

        #endregion

        #region Constructors/Creation

        public VesselPositionUpdateAlternative(VesselPositionMsgData msgData)
        {
            Id = Guid.NewGuid();
            PlanetTime = msgData.PlanetTime;
            SentTime = msgData.GameSentTime;
            VesselId = msgData.VesselId;
            BodyName = msgData.BodyName;
            TransformRotation = msgData.TransformRotation;
            OrbitPosition = msgData.OrbitPosition;
            Acceleration = msgData.Acceleration;
            WorldPosition = msgData.TransformPosition;
            LatLonAlt = msgData.LatLonAlt;
            OrbitVelocity = msgData.OrbitVelocity;
            Velocity = msgData.Velocity;
            Orbit = msgData.Orbit;
        }

        public VesselPositionUpdateAlternative(Vessel vessel)
        {
            try
            {
                VesselId = vessel.id;
                PlanetTime = Planetarium.GetUniversalTime();
                BodyName = vessel.mainBody.bodyName;
                //Update the vessel's orbital information from the current position of the rigidbodies
                vessel.orbitDriver.TrackRigidbody(vessel.mainBody, 0);
                vessel.UpdatePosVel();

                TransformRotation = new[]
                {
                    vessel.vesselTransform.rotation.x,
                    vessel.vesselTransform.rotation.y,
                    vessel.vesselTransform.rotation.z,
                    vessel.vesselTransform.rotation.w
                };
                Acceleration = new[]
                {
                    vessel.acceleration.x,
                    vessel.acceleration.y,
                    vessel.acceleration.z
                };
                OrbitPosition = new[]
                {
                    vessel.orbit.pos.x,
                    vessel.orbit.pos.y,
                    vessel.orbit.pos.z
                };
                LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude
                };
                var worldPosition = vessel.GetWorldPos3D();
                WorldPosition = new[]
                {
                    worldPosition.x,
                    worldPosition.y,
                    worldPosition.z
                };
                Vector3d srfVel = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
                Velocity = new[]
                {
                    Math.Abs(Math.Round(srfVel.x, 2)) < 0.01 ? 0 : srfVel.x,
                    Math.Abs(Math.Round(srfVel.y, 2)) < 0.01 ? 0 : srfVel.y,
                    Math.Abs(Math.Round(srfVel.z, 2)) < 0.01 ? 0 : srfVel.z
                };
                var orbitVel = vessel.orbit.GetVel();
                OrbitVelocity = new[]
                {
                    orbitVel.x,
                    orbitVel.y,
                    orbitVel.z
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
                Debug.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
            }
        }

        public virtual VesselPositionUpdate Clone()
        {
            return MemberwiseClone() as VesselPositionUpdate;
        }

        #endregion

        #region Main method

        /// <summary>
        /// Apply the vessel update.  Run on FixedUpdate as we're updating physics, therefore it cannot be run in update()
        /// </summary>
        /// <returns></returns>
        public void ApplyVesselUpdate()
        {
            if (Body == null)
            {
                Body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
            }
            if (Vessel == null)
            {
                Vessel = FlightGlobals.Vessels.FindLast(v => v.id == VesselId);
            }

            if (Body != null && Vessel != null)
            {
                PlanetariumDifference = Planetarium.GetUniversalTime() - PlanetTime;

                ApplyVesselPositionAndRotation();
                ApplyVesselPosition();

                //Calculate the srfRelRotation, height from terrain, radar altitude, altitude, and orbit values from the various items set in the above methods.
                //Vessel.UpdatePosVel();
            }
        }

        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void ApplyVesselPositionAndRotation()
        {
            var targetTransformRot = new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);

            Vessel.vesselTransform.position = SurfacePosition;
            Vessel.vesselTransform.rotation = targetTransformRot;
            Vessel.srfRelRotation = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * Vessel.vesselTransform.rotation;
            Vessel.precalc.worldSurfaceRot = Vessel.mainBody.bodyTransform.rotation * Vessel.srfRelRotation;

            //If vessel is packed then set the position (it will call vessel.SetPosition(vessel.vesselTransform.position, true))
            //If the vessel is NOT packed WE must call vessel.SetPosition(pos, FALSE)
            Vessel.SetRotation(targetTransformRot, Vessel.packed);
        }

        /// <summary>
        /// Applies surface interpolation values
        /// </summary>
        private void ApplyVesselPosition()
        {
            Vessel.latitude = LatLonAlt[0];
            Vessel.longitude = LatLonAlt[1];
            Vessel.altitude = LatLonAlt[2];

            //Update the protovessel, so if the vessel gets saved out, it's at the right spot
            Vessel.protoVessel.latitude = LatLonAlt[0];
            Vessel.protoVessel.longitude = LatLonAlt[1];
            Vessel.protoVessel.altitude = LatLonAlt[2];
            Vessel.protoVessel.position = Vessel.vesselTransform.position;
            Vessel.precalc.worldSurfacePos = SurfacePosition;

            if (!Vessel.packed) //Call SetPosition ONLY when vessel is unpacked!
                Vessel.SetPosition(SurfacePosition, false);
        }

        //Credit where credit is due, Thanks hyperedit.
        private static void CopyOrbit(Orbit sourceOrbit, Orbit destinationOrbit)
        {
            destinationOrbit.inclination = sourceOrbit.inclination;
            destinationOrbit.eccentricity = sourceOrbit.eccentricity;
            destinationOrbit.semiMajorAxis = sourceOrbit.semiMajorAxis;
            destinationOrbit.LAN = sourceOrbit.LAN;
            destinationOrbit.argumentOfPeriapsis = sourceOrbit.argumentOfPeriapsis;
            destinationOrbit.meanAnomalyAtEpoch = sourceOrbit.meanAnomalyAtEpoch;
            destinationOrbit.epoch = sourceOrbit.epoch;
            destinationOrbit.referenceBody = sourceOrbit.referenceBody;
            destinationOrbit.Init();
            destinationOrbit.UpdateFromUT(Planetarium.GetUniversalTime());
        }

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        private static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        #endregion
    }
}

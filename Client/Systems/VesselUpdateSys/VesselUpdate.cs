using System;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// This class handle the vessel updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselUpdate
    {
        #region Fields

        #region Vessel information fields

        public Guid VesselId { get; set; }
        public double PlanetTime { get; set; }
        public string BodyName { get; set; }
        public float[] Rotation { get; set; }
        public float[] AngularVel { get; set; }
        public FlightCtrlState FlightState { get; set; }
        public bool[] ActionGrpControls { get; set; }
        public bool IsSurfaceUpdate { get; set; }

        #region Orbit field

        public double[] Orbit { get; set; }

        #endregion

        #region Surface fields

        public double[] Position { get; set; }
        public double[] Velocity { get; set; }
        public double[] Acceleration { get; set; }

        #endregion

        #endregion

        #region Interpolation fields

        public long SentTime { get; set; }
        public long InterpolationStartTime { get; set; }
        public long InterpolationFinishTime { get; set; }
        public bool InterpolationStarted { get; set; }
        public bool InterpolationFinished => InterpolationPercentage >= 1;
        public float InterpolationPercentage => (float)(DateTime.UtcNow.Ticks - InterpolationStartTime) / InterpolationDuration;
        public long InterpolationDuration { get; set; }

        #endregion

        #region Private fields

        private Vessel Vessel { get; set; }
        private CelestialBody Body { get; set; }
        private VesselUpdate NextUpdate { get; set; }

        #endregion

        #endregion

        #region Creation methods

        public static VesselUpdate CreateFromVesselId(Guid vesselId)
        {
            var vessel = FlightGlobals.Vessels.FindLast(v => v.id == vesselId);
            return vessel != null ? CreateFromVessel(vessel) : null;
        }

        public static VesselUpdate CreateFromVessel(Vessel vessel)
        {
            try
            {
                var returnUpdate = new VesselUpdate
                {
                    VesselId = vessel.id,
                    PlanetTime = Planetarium.GetUniversalTime(),
                    FlightState = new FlightCtrlState(),
                    BodyName = vessel.mainBody.bodyName,
                    Rotation = new[]
                    {
                        vessel.srfRelRotation.x,
                        vessel.srfRelRotation.y,
                        vessel.srfRelRotation.z,
                        vessel.srfRelRotation.w
                    },
                    AngularVel = new[]
                    {
                        vessel.angularVelocity.x,
                        vessel.angularVelocity.y,vessel.angularVelocity.z
                    },
                    ActionGrpControls = new[]
                    {
                        vessel.ActionGroups[KSPActionGroup.Gear],
                        vessel.ActionGroups[KSPActionGroup.Light],
                        vessel.ActionGroups[KSPActionGroup.Brakes],
                        vessel.ActionGroups[KSPActionGroup.SAS],
                        vessel.ActionGroups[KSPActionGroup.RCS]
                    }
                };
                
                returnUpdate.FlightState.CopyFrom(vessel.ctrlState);
                
                if (vessel.altitude < 10000)
                {
                    //Use surface position under 10k
                    returnUpdate.IsSurfaceUpdate = true;

                    returnUpdate.Position = new[]
                    {
                        vessel.latitude,
                        vessel.longitude,
                        vessel.altitude,
                        vessel.radarAltitude
                    };

                    Vector3d srfVel = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
                    returnUpdate.Velocity = new[]
                    {
                        srfVel.x,
                        srfVel.y,
                        srfVel.z
                    };

                    Vector3d srfAcceleration = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.acceleration;
                    returnUpdate.Acceleration = new[]
                    {
                        srfAcceleration.x,
                        srfAcceleration.y,
                        srfAcceleration.z
                    };
                }
                else
                {
                    //Use orbital positioning over 10k
                    returnUpdate.IsSurfaceUpdate = false;

                    returnUpdate.Orbit = new[]
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
                return returnUpdate;
            }
            catch (Exception e)
            {
                LunaLog.Debug("Failed to get vessel update, exception: " + e);
                return null;
            }
        }

        #endregion

        #region Main interpolation method

        public void ApplyVesselUpdate(VesselUpdate nextUpdate)
        {
            Body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
            Vessel = FlightGlobals.Vessels.FindLast(v => v.id == VesselId);
            NextUpdate = nextUpdate;

            if ((Body == null) || (Vessel == null))
                return;

            if (!InterpolationStarted)
            {
                InterpolationStarted = true;
                InterpolationDuration += nextUpdate.SentTime - SentTime;
            }

            //This value varies a lot depending on the current tick so fix it.
            var interpolationValue = InterpolationPercentage;

            if (interpolationValue >= 0.5)
            {
                //Set the action groups when we are past the middle of the interpolation
                Vessel.ActionGroups.SetGroup(KSPActionGroup.Gear, nextUpdate.ActionGrpControls[0]);
                Vessel.ActionGroups.SetGroup(KSPActionGroup.Light, nextUpdate.ActionGrpControls[1]);
                Vessel.ActionGroups.SetGroup(KSPActionGroup.Brakes, nextUpdate.ActionGrpControls[2]);
                Vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, nextUpdate.ActionGrpControls[3]);
                Vessel.ActionGroups.SetGroup(KSPActionGroup.RCS, nextUpdate.ActionGrpControls[4]);

                //Set also the current flight state (position of ailerons, gear, etc)
                if (!VesselLockSystem.Singleton.IsSpectating)
                    Vessel.ctrlState.CopyFrom(FlightState);
                else
                    FlightInputHandler.state.CopyFrom(FlightState);
            }

            ApplyCommonInterpolation(interpolationValue);

            if (IsSurfaceUpdate)
                ApplySurfaceInterpolation(interpolationValue);
            else
                ApplyOrbitInterpolation(interpolationValue);
        }

        #endregion

        #region Private interpolation methods


        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void ApplyCommonInterpolation(float interpolationValue)
        {
            if (interpolationValue > 1) return;

            //Here we pick the starting point, the finishing points and we pick the current values by calling the Lerp functions
            //Lerp will give you a point in the middle based on the interpolationValue (0 to 1). 
            //Bear in mind that for rotations you must use Slerp!

            var startRot = new Quaternion(Rotation[0], Rotation[1], Rotation[2], Rotation[3]);
            var targetRot = new Quaternion(NextUpdate.Rotation[0], NextUpdate.Rotation[1], NextUpdate.Rotation[2], NextUpdate.Rotation[3]);

            var startAngVel = new Vector3(AngularVel[0], AngularVel[1], AngularVel[2]);
            var targetAngVel = new Vector3(NextUpdate.AngularVel[0], NextUpdate.AngularVel[1], NextUpdate.AngularVel[2]);

            var currentRot = Body.bodyTransform.rotation * Quaternion.identity *
                              Quaternion.Slerp(startRot, targetRot, InterpolationPercentage);

            var currentAngVel = Vessel.mainBody.bodyTransform.rotation * currentRot *
                                Vector3.Lerp(startAngVel, targetAngVel, InterpolationPercentage);

            Vessel.SetRotation(currentRot);
            Vessel.angularVelocity = currentAngVel;

            if (Vessel.packed)
            {
                Vessel.srfRelRotation = currentRot;
                Vessel.protoVessel.rotation = Vessel.srfRelRotation;
            }
        }

        /// <summary>
        /// Applies surface interpolation values
        /// </summary>
        private void ApplySurfaceInterpolation(float interpolationValue)
        {
            if (interpolationValue > 1) return;

            var startVel = new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
            var targetVel = new Vector3d(NextUpdate.Velocity[0], NextUpdate.Velocity[1], NextUpdate.Velocity[2]);

            var startAcc = new Vector3d(Acceleration[0], Acceleration[1], Acceleration[2]);
            var targetAcc = new Vector3d(NextUpdate.Acceleration[0], NextUpdate.Acceleration[1], NextUpdate.Acceleration[2]);

            var lat = Lerp(Position[0], NextUpdate.Position[0], InterpolationPercentage);
            var lon = Lerp(Position[1], NextUpdate.Position[1], InterpolationPercentage);
            var alt = Lerp(Position[2], NextUpdate.Position[2], InterpolationPercentage);
            var radarAlt = Lerp(Position[3], NextUpdate.Position[3], InterpolationPercentage);

            Vector3d currentPosition = Body.GetWorldSurfacePosition(lat, lon, alt);
            Vector3d currentVelocity = Body.bodyTransform.rotation * Vector3d.Lerp(startVel, targetVel, InterpolationPercentage);
            Vector3d currentAcc = Body.bodyTransform.rotation * Vector3d.Lerp(startAcc, targetAcc, InterpolationPercentage);

            Vessel.SetPosition(currentPosition, true);
            Vessel.ChangeWorldVelocity(currentVelocity - Vessel.srf_velocity);
            Vessel.acceleration = currentAcc;

            if (radarAlt < 10) //Only apply radar altitud for vessels on the ground
                Vessel.radarAltitude = radarAlt;
        }

        /// <summary>
        /// Applies interpolation when above 10000m
        /// </summary>
        /// <param name="interpolationValue"></param>
        private void ApplyOrbitInterpolation(float interpolationValue)
        {
            if (interpolationValue > 1) return;

            var startOrbit = new Orbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3], Orbit[4], Orbit[5], Orbit[6], Body);
            var targetOrbit = new Orbit(NextUpdate.Orbit[0], NextUpdate.Orbit[1], NextUpdate.Orbit[2], NextUpdate.Orbit[3], NextUpdate.Orbit[4],
                NextUpdate.Orbit[5], NextUpdate.Orbit[6], Body);

            var currentOrbit = OrbitLerp(startOrbit, targetOrbit, Body, interpolationValue);

            currentOrbit.Init();
            currentOrbit.UpdateFromUT(Planetarium.GetUniversalTime());

            var latitude = Body.GetLatitude(currentOrbit.pos);
            var longitude = Body.GetLongitude(currentOrbit.pos);
            var altitude = Body.GetAltitude(currentOrbit.pos);

            Vessel.latitude = latitude;
            Vessel.longitude = longitude;
            Vessel.altitude = altitude;
            Vessel.protoVessel.latitude = latitude;
            Vessel.protoVessel.longitude = longitude;
            Vessel.protoVessel.altitude = altitude;

            if (Vessel.packed)
            {
                //The OrbitDriver update call will set the vessel position on the next fixed update
                CopyOrbit(currentOrbit, Vessel.orbitDriver.orbit);
                Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
                Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel;
            }
            else
            {
                var posDelta = currentOrbit.getPositionAtUT(Planetarium.GetUniversalTime()) -
                               Vessel.orbitDriver.orbit.getPositionAtUT(Planetarium.GetUniversalTime());

                var velDelta = currentOrbit.getOrbitalVelocityAtUT(Planetarium.GetUniversalTime()).xzy -
                               Vessel.orbitDriver.orbit.getOrbitalVelocityAtUT(Planetarium.GetUniversalTime()).xzy;

                Vessel.Translate(posDelta);
                Vessel.ChangeWorldVelocity(velDelta);
            }
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
        /// Custom lerp for orbits
        /// </summary>
        private static Orbit OrbitLerp(Orbit startOrbit, Orbit endOrbit, CelestialBody body, float interpolationValue)
        {
            var inc = Lerp(startOrbit.inclination, endOrbit.inclination, interpolationValue);
            var e = Lerp(startOrbit.eccentricity, endOrbit.eccentricity, interpolationValue);
            var sma = Lerp(startOrbit.semiMajorAxis, endOrbit.semiMajorAxis, interpolationValue);
            var lan = Lerp(startOrbit.LAN, endOrbit.LAN, interpolationValue);
            var argPe = Lerp(startOrbit.argumentOfPeriapsis, endOrbit.argumentOfPeriapsis, interpolationValue);
            var mEp = Lerp(startOrbit.meanAnomalyAtEpoch, endOrbit.meanAnomalyAtEpoch, interpolationValue);
            var t = Lerp(startOrbit.epoch, endOrbit.epoch, interpolationValue);

            var orbitLerp = new Orbit(inc, e, sma, lan, argPe, mEp, t, body);
            return orbitLerp;
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
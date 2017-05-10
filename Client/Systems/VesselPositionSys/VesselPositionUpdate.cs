using System;
using System.Collections;
using LunaClient.Systems.SettingsSys;
using UnityEngine;
using LunaCommon.Message.Data.Vessel;

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionUpdate
    {
        #region Fields

        private float LerpPercentage { get; set; }
        public Vessel Vessel { get; set; }
        public CelestialBody Body { get; set; }
        public VesselPositionUpdate Target { get; set; }
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

        #endregion

        #endregion

        #region Interpolation fields

        public float SentTime { get; set; }
        public bool InterpolationStarted { get; set; }
        public bool InterpolationFinished { get; set; }
        public float FinishTime { get; set; }

        #endregion

        #region Private fields

        private float _interpolationDuration;
        private double PlanetariumDifference { get; set; }
        private const float PlanetariumDifferenceLimit = 3f;

        #endregion

        #endregion

        #region Constructors/Creation

        public VesselPositionUpdate(VesselPositionMsgData msgData)
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

        public VesselPositionUpdate(Vessel vessel)
        {
            try
            {
                VesselId = vessel.id;
                PlanetTime = Planetarium.GetUniversalTime();
                BodyName = vessel.mainBody.bodyName;
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
                OrbitPosition = new double[]
                {
                    vessel.orbit.pos.x,
                    vessel.orbit.pos.y,
                    vessel.orbit.pos.z
                };
                LatLonAlt = new double[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude
                };
                WorldPosition = new double[]
                {
                    vessel.GetWorldPos3D().x,
                    vessel.GetWorldPos3D().y,
                    vessel.GetWorldPos3D().z
                };
                Vector3d srfVel = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
                Velocity = new[]
                {
                    Math.Abs(Math.Round(vessel.rb_velocityD.x, 2)) < 0.01 ? 0 : vessel.rb_velocityD.x,
                    Math.Abs(Math.Round(vessel.rb_velocityD.y, 2)) < 0.01 ? 0 : vessel.rb_velocityD.y,
                    Math.Abs(Math.Round(vessel.rb_velocityD.z, 2)) < 0.01 ? 0 : vessel.rb_velocityD.z,
                };
                OrbitVelocity = new[]
                {
                    vessel.orbit.vel.x,
                    vessel.orbit.vel.y,
                    vessel.orbit.vel.z,
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


        public VesselPositionUpdate(Guid vesselId) : this(FlightGlobals.Vessels.FindLast(v => v.id == vesselId))
        {
        }

        public virtual VesselPositionUpdate Clone()
        {
            return this.MemberwiseClone() as VesselPositionUpdate;
        }

        #endregion

        #region Main interpolation method

        /// <summary>
        /// This coroutine is run at every fixed update as we are updating rigid bodies (physics are involved)
        /// therefore we cannot use it in Update()
        /// </summary>
        /// <returns></returns>
        public void ApplyVesselUpdate()
        {
            if (!InterpolationStarted)
            {
                StartupInterpolation();
            }

            if (Body != null && Vessel != null && _interpolationDuration > 0)
            {
                if (SettingsSystem.CurrentSettings.InterpolationEnabled && LerpPercentage < 1)
                {
                    ApplyInterpolations(LerpPercentage);
                    LerpPercentage += Time.fixedDeltaTime / _interpolationDuration;
                    return;
                }

                if (!SettingsSystem.CurrentSettings.InterpolationEnabled)
                {
                    ApplyInterpolations(1);
                }
            }

            FinishInterpolation();
        }

        #endregion

        #region Private interpolation methods

        /// <summary>
        /// Finish the interpolation
        /// </summary>
        private void FinishInterpolation()
        {
            InterpolationFinished = true;
            FinishTime = Time.time;
        }

        /// <summary>
        /// Start the interpolation and set it's needed values
        /// </summary>
        private void StartupInterpolation()
        {
            if (Body == null)
                Body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
            if (Vessel == null)
                Vessel = FlightGlobals.Vessels.FindLast(v => v.id == VesselId);

            InterpolationStarted = true;

            if (Body != null && Vessel != null)
            {
                PlanetariumDifference = Planetarium.GetUniversalTime() - PlanetTime;

                //Here we use the interpolation factor to make the interpolation duration 
                //shorter or longer depending on the amount of updates we have in queue.
                //We never exceed the MaxSInterpolationTime
                _interpolationDuration = Target.SentTime - SentTime - VesselPositionInterpolationSystem.GetInterpolationFactor(VesselId);
                _interpolationDuration = Mathf.Clamp(_interpolationDuration, 0, VesselPositionInterpolationSystem.MaxSInterpolationTime);
            }
        }

        /// <summary>
        /// Apply the interpolation based on a percentage
        /// </summary>
        private void ApplyInterpolations(float percentage)
        {
            if (Vessel == null || percentage > 1) return;

            ApplyRotationInterpolation(percentage);
            ApplySurfaceInterpolation(percentage);
            //ApplyOrbitInterpolation(percentage);
        }

        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void ApplyRotationInterpolation(float interpolationValue)
        {
            var startTransformRot = new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);
            var targetTransformRot = new Quaternion(Target.TransformRotation[0], Target.TransformRotation[1], Target.TransformRotation[2], Target.TransformRotation[3]);
            var currentTransformRot = Quaternion.Slerp(startTransformRot, targetTransformRot, interpolationValue);

            Vessel.SetRotation(currentTransformRot);
            Vessel.vesselTransform.rotation = currentTransformRot;
            Vessel.srfRelRotation = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * Vessel.vesselTransform.rotation;
            Vessel.precalc.worldSurfaceRot = Vessel.mainBody.bodyTransform.rotation * Vessel.srfRelRotation;
        }

        /// <summary>
        /// Here we get the interpolated velocity. 
        /// We should fudge it as we are seeing the client IN THE PAST so we need to extrapolate the speed 
        /// </summary>
        private Vector3d GetInterpolatedVelocity(float interpolationValue, Vector3d acceleration)
        {
            var startVel = new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
            var targetVel = new Vector3d(Target.Velocity[0], Target.Velocity[1], Target.Velocity[2]);
            var currentVel = Body.bodyTransform.rotation * Vector3d.Lerp(startVel, targetVel, interpolationValue);

            if (SettingsSystem.CurrentSettings.PositionFudgeEnable)
            {
                //Velocity = a*t
                var velocityFudge = acceleration * PlanetariumDifference;
                return currentVel + velocityFudge;
            }

            return currentVel;
        }

        /// <summary>
        /// Here we get the interpolated acceleration
        /// </summary>
        private Vector3d GetInterpolatedAcceleration(float interpolationValue)
        {
            var startAcc = new Vector3d(Acceleration[0], Acceleration[1], Acceleration[2]);
            var targetAcc = new Vector3d(Target.Acceleration[0], Target.Acceleration[1], Target.Acceleration[2]);
            Vector3d currentAcc = Body.bodyTransform.rotation * Vector3d.Lerp(startAcc, targetAcc, interpolationValue);
            return currentAcc;
        }

        /// <summary>
        /// Applies surface interpolation values
        /// </summary>
        private void ApplySurfaceInterpolation(float interpolationValue)
        {
            var currentAcc = GetInterpolatedAcceleration(interpolationValue);
            var currentVelocity = GetInterpolatedVelocity(interpolationValue, currentAcc);

            Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], interpolationValue);
            Vessel.longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], interpolationValue);
            Vessel.altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], interpolationValue);
            var worldSurfacePosition = Vessel.mainBody.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude);

            if (SettingsSystem.CurrentSettings.PositionFudgeEnable)
            {
                //Use the average velocity to determine the new position --- Displacement = v0*t + 1/2at^2.
                var positionFudge = (currentVelocity * PlanetariumDifference) + (0.5d * currentAcc * PlanetariumDifference * PlanetariumDifference);
                worldSurfacePosition += positionFudge;
            }

            var startWorldPos = new Vector3d(WorldPosition[0], WorldPosition[1], WorldPosition[2]);
            var targetWorldPos = new Vector3d(Target.WorldPosition[0], Target.WorldPosition[1], Target.WorldPosition[2]);
            var currentWorldPos = Vector3d.Lerp(startWorldPos, targetWorldPos, interpolationValue);

            Vessel.SetPosition(worldSurfacePosition, true);
            Vessel.CoMD = currentWorldPos;

            var startOrbitPos = new Vector3d(OrbitPosition[0], OrbitPosition[1], OrbitPosition[2]);
            var targetOrbitPos = new Vector3d(Target.OrbitPosition[0], Target.OrbitPosition[1], Target.OrbitPosition[2]);
            var currentOrbitPos = Vector3d.Lerp(startOrbitPos, targetOrbitPos, interpolationValue);
            Vessel.orbit.pos = currentOrbitPos;

            Vessel.SetWorldVelocity(currentVelocity);

            var startObtVel = new Vector3d(OrbitVelocity[0], OrbitVelocity[1], OrbitVelocity[2]);
            var targetObtVel = new Vector3d(Target.OrbitVelocity[0], Target.OrbitVelocity[1], Target.OrbitVelocity[2]);
            var currentObtVel = Vector3d.Lerp(startObtVel, targetObtVel, interpolationValue);
            Vessel.orbit.vel = currentObtVel;
        }

        /// <summary>
        /// Applies interpolation when above 10000m
        /// </summary>
        private void ApplyOrbitInterpolation(float interpolationValue)
        {
            var startOrbit = new Orbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3], Orbit[4], Orbit[5], Orbit[6], Body);
            var targetOrbit = new Orbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3], Target.Orbit[4],
                Target.Orbit[5], Target.Orbit[6], Body);

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

        /// <summary>
        /// Custom lerp for a flight control state
        /// </summary>
        private static FlightCtrlState Lerp(FlightCtrlState from, FlightCtrlState to, float t)
        {
            return new FlightCtrlState
            {
                X = Mathf.Lerp(from.X, to.X, t),
                Y = Mathf.Lerp(from.Y, to.Y, t),
                Z = Mathf.Lerp(from.Z, to.Z, t),
                gearDown = t < 0.5 ? from.gearDown : to.gearDown,
                gearUp = t < 0.5 ? from.gearUp : to.gearUp,
                headlight = t < 0.5 ? from.headlight : to.headlight,
                killRot = t < 0.5 ? from.killRot : to.killRot,
                mainThrottle = Mathf.Lerp(from.mainThrottle, to.mainThrottle, t),
                pitch = Mathf.Lerp(from.pitch, to.pitch, t),
                roll = Mathf.Lerp(from.roll, to.roll, t),
                yaw = Mathf.Lerp(from.yaw, to.yaw, t),
                pitchTrim = Mathf.Lerp(from.pitchTrim, to.pitchTrim, t),
                rollTrim = Mathf.Lerp(from.rollTrim, to.rollTrim, t),
                yawTrim = Mathf.Lerp(from.yawTrim, to.yawTrim, t),
                wheelSteer = Mathf.Lerp(from.wheelSteer, to.wheelSteer, t),
                wheelSteerTrim = Mathf.Lerp(from.wheelSteerTrim, to.wheelSteerTrim, t),
                wheelThrottle = Mathf.Lerp(from.wheelThrottle, to.wheelThrottle, t),
                wheelThrottleTrim = Mathf.Lerp(from.wheelThrottleTrim, to.wheelThrottleTrim, t),
            };
        }

        private static Vector3 Round(Vector3 vector, int decimals)
        {
            return new Vector3((float)Math.Round(vector.x, decimals), (float)Math.Round(vector.y, decimals), (float)Math.Round(vector.z, decimals));
        }

        #endregion
    }
}
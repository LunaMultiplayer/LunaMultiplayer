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
        public float ReceiveTime { get; set; }
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
            ReceiveTime = Time.fixedTime;
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
                Vector3d worldPosition = vessel.GetWorldPos3D();
                WorldPosition = new double[]
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
                    Math.Abs(Math.Round(srfVel.z, 2)) < 0.01 ? 0 : srfVel.z,
                    //Math.Abs(Math.Round(vessel.velocityD.x, 2)) < 0.01 ? 0 : vessel.velocityD.x,
                    //Math.Abs(Math.Round(vessel.velocityD.y, 2)) < 0.01 ? 0 : vessel.velocityD.y,
                    //Math.Abs(Math.Round(vessel.velocityD.z, 2)) < 0.01 ? 0 : vessel.velocityD.z,
                };
                Vector3d orbitVel = vessel.orbit.GetVel();
                OrbitVelocity = new[]
                {
                    orbitVel.x,
                    orbitVel.y,
                    orbitVel.z
                    //vessel.orbit.vel.x,
                    //vessel.orbit.vel.y,
                    //vessel.orbit.vel.z,
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
            percentage = 1.0f;
            if (Vessel == null || percentage > 1) return;

            Vessel.Landed = false;
            Vessel.Splashed = false;
            ApplyRotationInterpolation(percentage);
            ApplySurfaceInterpolation(percentage);
            //ApplyOrbitInterpolation(percentage);

            //Calculate the srfRelRotation, height from terrain, radar altitude, altitude, and orbit values from the various items set in the above methods.
            //Vessel.UpdatePosVel();
        }

        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void ApplyRotationInterpolation(float interpolationValue)
        {
            var startTransformRot = new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);
            var targetTransformRot = new Quaternion(Target.TransformRotation[0], Target.TransformRotation[1], Target.TransformRotation[2], Target.TransformRotation[3]);
            var currentTransformRot = Quaternion.Slerp(startTransformRot, targetTransformRot, interpolationValue);

            if (SettingsSystem.CurrentSettings.Debug7)
            {
                Vessel.vesselTransform.rotation = currentTransformRot;
                Vessel.SetRotation(currentTransformRot, true);
            } else
            {
                //Debug.LogError(Target.TransformRotation[0] + "," + Target.TransformRotation[1] + "," + Target.TransformRotation[2] + "," + Target.TransformRotation[3]);
            }
        }

        /// <summary>
        /// Here we get the interpolated velocity. 
        /// We should fudge it as we are seeing the client IN THE PAST so we need to extrapolate the speed 
        /// </summary>
        private Vector3d GetInterpolatedVelocity(float interpolationValue, Vector3d acceleration)
        {
            var startVel = new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
            //var targetVel = new Vector3d(Target.Velocity[0], Target.Velocity[1], Target.Velocity[2]);
            //var targetVel = Body.bodyTransform.rotation * new Vector3d(Target.Velocity[0], Target.Velocity[1], Target.Velocity[2]);
            //var currentVel = Body.bodyTransform.rotation * Vector3d.Lerp(startVel, targetVel, interpolationValue);

            //KSP reverses the two last components of the orbit vector.  Don't ask me why...
            var targetVel = new Vector3d(Target.OrbitVelocity[0], Target.OrbitVelocity[1], Target.OrbitVelocity[2]);

            if (SettingsSystem.CurrentSettings.PositionFudgeEnable)
            {
                //Velocity = a*t
                //var velocityFudge = acceleration * PlanetariumDifference;
                //return currentVel + velocityFudge;
            }

            return targetVel;
            //return targetVel - (FlightGlobals.ActiveVessel.srf_velocity - FlightGlobals.ActiveVessel.rb_velocity);
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
            //May need to call TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Late, aMethod) to call this at the end of FixedUpdate.
            //It'll happen after the FlightIntegrator code, at least.


            //var currentAcc = GetInterpolatedAcceleration(interpolationValue);
            var currentAcc = new Vector3d(0, 0, 0);
            var currentVelocity = GetInterpolatedVelocity(interpolationValue, currentAcc);

            var latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], interpolationValue);
            var longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], interpolationValue);
            var altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], interpolationValue);

            if (SettingsSystem.CurrentSettings.Debug4)
            {
                //TODO: Need to make sure position setting is working for landed vessels.
                //Update the vessels's surface position to ensure that it's at the right spot
                Vessel.latitude = latitude;
                Vessel.longitude = longitude;
                Vessel.altitude = altitude;

                //Update the protovessel, so if the vessel gets saved out, it's at the right spot
                Vessel.protoVessel.latitude = latitude;
                Vessel.protoVessel.longitude = longitude;
                Vessel.protoVessel.altitude = altitude;
            }

            Vector3d updatePosition = Body.GetWorldSurfacePosition(latitude, longitude, altitude);
            if (SettingsSystem.CurrentSettings.PositionFudgeEnable)
            {
                //Use the average velocity to determine the new position --- Displacement = v0*t + 1/2at^2.
                var positionFudge = (currentVelocity * PlanetariumDifference);
                updatePosition += positionFudge;
            }

            if (SettingsSystem.CurrentSettings.Debug1) {
                CheatOptions.NoCrashDamage = true;
                CheatOptions.UnbreakableJoints = true;
            }

            //TODO: Need to properly handle landed vessels
            if (Vessel.packed)
            {
                //TODO: Test--unproven code
                //TODO: Returns -1 when above the surface.  FIX.
                if (true || Vessel.GetHeightFromSurface() > 10)
                {
                    Vessel.Landed = false;
                    Vessel.Splashed = false;
                }
            }
            else
            {
                Vessel.Landed = false;
                Vessel.Splashed = false;
                //This doesn't work for packed vessels
                //Vessel.UpdateLandedSplashed();
            }
            
            if (!Vessel.LandedOrSplashed)
            {
                Vector3d orbitalPos = updatePosition - Vessel.mainBody.position;
                Vector3d orbitalVel = currentVelocity;

                if (SettingsSystem.CurrentSettings.Debug3)
                {
                    Vessel.orbitDriver.orbit.UpdateFromStateVectors(orbitalPos.xzy, orbitalVel.xzy, Body, Planetarium.GetUniversalTime());
                    Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
                    Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel.xzy;

                    if (SettingsSystem.CurrentSettings.Debug8)
                    {
                        //Set the velocity on each part and its rigidbody (if it exists)
                        Vector3d vesselUniverseVelocity = (Vessel.orbit.GetVel() - (!Vessel.orbit.referenceBody.inverseRotation ? Vector3d.zero : Vessel.orbit.referenceBody.getRFrmVel(Vessel.vesselTransform.position)));
                        Vector3d vel = vesselUniverseVelocity - Krakensbane.GetFrameVelocity();
                        int numParts = Vessel.Parts.Count;
                        for (int i = 0; i < numParts; i++)
                        {
                            Part item = this.Vessel.parts[i];
                            //This is based on the behavior of Part.ResumeVelocity()
                            item.vel = Vector3.zero;
                            if (!Vessel.LandedOrSplashed)
                            {
                                Vector3 partUniverseVelocity = (item.orbit.GetVel() - (!item.orbit.referenceBody.inverseRotation ? Vector3d.zero : item.orbit.referenceBody.getRFrmVel(item.partTransform.position)));
                                Vector3 newVelocity = partUniverseVelocity - Krakensbane.GetFrameVelocity();
                                item.vel = newVelocity;
                            }
                            if (item.rb != null)
                            {
                                item.rb.velocity = item.vel;
                            }

                        }
                        Vessel.angularVelocityD = Vector3d.zero;
                        Vessel.angularVelocity = Vector3.zero;
                    }
                }

                if (SettingsSystem.CurrentSettings.Debug9)
                {
                    Vessel.orbitDriver.updateFromParameters();
                    //After a single position update, reset the flag so that it only does one update per debug9 click
                    SettingsSystem.CurrentSettings.Debug9 = false;
                }
            }
            //Need to copy the control states for the vessels
            //Vessel.ctrlState.Neutralize();
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
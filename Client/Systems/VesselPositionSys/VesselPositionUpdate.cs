using System;
using System.Collections;
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

        public Vessel Vessel { get; set; }
        public CelestialBody Body { get; set; }
        public VesselPositionUpdate Target { get; set; }
        public Guid Id { get; set; }
        public double PlanetTime { get; set; }

        #region Vessel position information fields

        public Guid VesselId { get; set; }
        public string BodyName { get; set; }
        public float[] Rotation { get; set; }
        public bool IsSurfaceUpdate { get; set; }

        #region Orbit field

        public double[] Orbit { get; set; }

        #endregion

        #region Surface fields

        public double[] Position { get; set; }
        public double[] Velocity { get; set; }

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
            Rotation = msgData.Rotation;
            IsSurfaceUpdate = msgData.IsSurfaceUpdate;

            if (IsSurfaceUpdate)
            {
                Position = msgData.Position;
                Velocity = msgData.Velocity;
            }
            else
            {
                Orbit = msgData.Orbit;
            }
        }

        public VesselPositionUpdate(Vessel vessel)
        {
            try
            {
                VesselId = vessel.id;
                PlanetTime = Planetarium.GetUniversalTime();
                BodyName = vessel.mainBody.bodyName;
                Rotation = new[]
                {
                    vessel.srfRelRotation.x,
                    vessel.srfRelRotation.y,
                    vessel.srfRelRotation.z,
                    vessel.srfRelRotation.w
                };

                if (vessel.altitude < 30000)
                {
                    //Use surface position under 30k
                    IsSurfaceUpdate = true;

                    Position = new double[]
                    {
                        vessel.latitude,
                        vessel.longitude,
                        vessel.altitude,
                        vessel.radarAltitude
                    };

                    Vector3d srfVel = Quaternion.Inverse(vessel.mainBody.bodyTransform.rotation) * vessel.srf_velocity;
                    Velocity = new[]
                    {
                        srfVel.x,
                        srfVel.y,
                        srfVel.z
                    };
                }
                else
                {
                    //Use orbital positioning over 30k
                    IsSurfaceUpdate = false;

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
        public IEnumerator ApplyVesselUpdate()
        {
            var fixedUpdate = new WaitForFixedUpdate();
            if (!InterpolationStarted)
            {
                StartupInterpolation();
            }

            if (Body != null && Vessel != null && _interpolationDuration > 0)
            {
                //if (SettingsSystem.CurrentSettings.InterpolationEnabled)
                //{
                //    for (float lerp = 0; lerp < 1; lerp += Time.fixedDeltaTime / _interpolationDuration)
                //    {
                //        ApplyInterpolations(lerp);
                //        yield return fixedUpdate;
                //    }
                //}
                ApplyInterpolations(1); //we force to apply the last interpolation
                yield return fixedUpdate;
            }

            FinishInterpolation();
        }

        #endregion

        #region Overridable VesselPositionUpdate methods

        /// <summary>
        /// Sets any custom fields defined for this class
        /// </summary>
        protected virtual void CustomInterpolationStartupBehaviour()
        {
            //Implement your own behaviour
        }

        /// <summary>
        /// Applies the control state
        /// </summary>
        protected virtual void ApplyControlState(float percentage)
        {
        }

        public virtual VesselPositionMsgData CreateVesselUpdateMessage()
        {
            return new VesselPositionMsgData
            {
                GameSentTime = Time.fixedTime,
                PlanetTime = this.PlanetTime,
                VesselId = this.VesselId,
                BodyName = this.BodyName,
                IsSurfaceUpdate = this.IsSurfaceUpdate,
                Orbit = this.Orbit,
                Position = this.Position,
                Rotation = this.Rotation,
                Velocity = this.Velocity,
            };
        }

        #endregion

        #region Private interpolation methods

        /// <summary>
        /// Finish the interpolation
        /// </summary>
        private void FinishInterpolation()
        {
            try
            {
                InterpolationFinished = true;
                FinishTime = Time.time;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Coroutine error in FinishInterpolation {e}");
            }
        }

        /// <summary>
        /// Start the interpolation and set it's needed values
        /// </summary>
        private void StartupInterpolation()
        {
            try
            {
                if (Body == null)
                    Body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
                if (Vessel == null)
                    Vessel = FlightGlobals.Vessels.FindLast(v => v.id == VesselId);

                InterpolationStarted = true;

                if (Body != null && Vessel != null)
                {
                    PlanetariumDifference = Planetarium.GetUniversalTime() - PlanetTime;
                    CustomInterpolationStartupBehaviour();

                    //Here we use the interpolation facor to make the interpolation duration 
                    //shorter or longer depending on the amount of updates we have in queue.
                    //We never exceed the MaxSInterpolationTime
                    _interpolationDuration = Target.SentTime - SentTime - VesselPositionInterpolationSystem.GetInterpolationFactor(VesselId);
                    _interpolationDuration = Mathf.Clamp(_interpolationDuration, 0, VesselPositionInterpolationSystem.MaxSInterpolationTime);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Coroutine error in StartupInterpolation {e}");
            }
        }

        /// <summary>
        /// Apply the interpolation based on a percentage
        /// </summary>
        private void ApplyInterpolations(float percentage)
        {
            try
            {
                if (Vessel == null || percentage > 1) return;

                ApplyRotationInterpolation(percentage);

                if (IsSurfaceUpdate)
                    ApplySurfaceInterpolation(percentage);
                else
                    ApplyOrbitInterpolation(percentage);

                ApplyControlState(percentage);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Coroutine error in ApplyInterpolations {e}");
            }
        }

        private void ClearAngularVelocity()
        {
            Vector3 angularVelocity = new Vector3(0, 0, 0);
            //Vector3 angularVelocity = updateVessel.mainBody.bodyTransform.rotation * updateRotation * new Vector3(this.angularVelocity[0], this.angularVelocity[1], this.angularVelocity[2]);
            if (Vessel.parts != null)
            {
                foreach (Part vesselPart in Vessel.parts)
                {
                    if (vesselPart.rb != null && !vesselPart.rb.isKinematic && vesselPart.State == PartStates.ACTIVE)
                    {
                        vesselPart.rb.angularVelocity = angularVelocity;
                        if (vesselPart != Vessel.rootPart)
                        {
                            Vector3 rootPos = FlightGlobals.ActiveVessel.rootPart.rb.position;
                            Vector3 rootVel = FlightGlobals.ActiveVessel.rootPart.rb.velocity;
                            Vector3 diffPos = vesselPart.rb.position - rootPos;
                            Vector3 partVelDifference = Vector3.Cross(angularVelocity, diffPos);
                            vesselPart.rb.velocity = rootVel + partVelDifference;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void ApplyRotationInterpolation(float interpolationValue)
        {
            var startRot = new Quaternion(Rotation[0], Rotation[1], Rotation[2], Rotation[3]);
            var targetRot = new Quaternion(Target.Rotation[0], Target.Rotation[1], Target.Rotation[2], Target.Rotation[3]);

            var currentRot = targetRot;
            //var currentRot = Quaternion.Slerp(startRot, targetRot, interpolationValue);

            //Rotation
            Vessel.SetRotation(Vessel.mainBody.bodyTransform.rotation * currentRot);
            if (Vessel.packed)
            {
                Vessel.srfRelRotation = currentRot;
                Vessel.protoVessel.rotation = Vessel.srfRelRotation;
            }

            //TODO: Need to set the angular velocity on all parts to 0.
            //ClearAngularVelocity();
        }

        /// <summary>
        /// Here we get the interpolated velocity. 
        /// We should fudge it as we are seeing the client IN THE PAST so we need to extrapolate the speed 
        /// </summary>
        private Vector3d GetInterpolatedVelocity(float interpolationValue)
        {
            var startVel = new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
            var targetVel = new Vector3d(Target.Velocity[0], Target.Velocity[1], Target.Velocity[2]);

            //Velocity = a*t
            //var velocityFudge = acceleration*PlanetariumDifference;

            return Body.bodyTransform.rotation * Vector3d.Lerp(startVel, targetVel, interpolationValue);
            //return Body.bodyTransform.rotation*Vector3d.Lerp(startVel + velocityFudge, targetVel + velocityFudge, interpolationValue);
        }

        /// <summary>
        /// Applies surface interpolation values
        /// </summary>
        private void ApplySurfaceInterpolation(float interpolationValue)
        {
            var updateVelocity = GetInterpolatedVelocity(interpolationValue);

            Vector3d positionExtrapolation = (updateVelocity * PlanetariumDifference);

            var latitude = Lerp(Position[0], Target.Position[0], interpolationValue);
            var longitude = Lerp(Position[1], Target.Position[1], interpolationValue);
            var altitude = Lerp(Position[2], Target.Position[2], interpolationValue);

            //Extrapolate the position based on the time difference from the latest update
            //Vector3d updatePosition = Body.GetWorldSurfacePosition(latitude, longitude, altitude) + positionExtrapolation;
            //latitude = Body.GetLatitude(updatePosition);
            //longitude = Body.GetLongitude(updatePosition);
            //altitude = Body.GetAltitude(updatePosition);

            Vessel.latitude = latitude;
            Vessel.longitude = longitude;
            Vessel.altitude = altitude;
            Vessel.protoVessel.latitude = latitude;
            Vessel.protoVessel.longitude = longitude;
            Vessel.protoVessel.altitude = altitude;
            //Vessel.radarAltitude = Lerp(Position[3], Target.Position[3], interpolationValue);

            //Need to refer to the world surface position so that timewarp are handled.  The position moves over time.
            Vector3d updatePostion = Body.GetWorldSurfacePosition(latitude, longitude, altitude);


            if (Vessel.packed)
            {
                if (!Vessel.LandedOrSplashed)
                {
                    //Not landed but under 10km.
                    Vector3d orbitalPos = updatePostion - Body.position;
                    Vector3d surfaceOrbitVelDiff = Body.getRFrmVel(updatePostion);
                    Vector3d orbitalVel = updateVelocity + surfaceOrbitVelDiff;
                    Vessel.orbitDriver.orbit.UpdateFromStateVectors(orbitalPos.xzy, orbitalVel.xzy, Body, Planetarium.GetUniversalTime());
                    Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
                    Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel;
                }
            }
            else
            {
                Vector3d velocityOffset = updateVelocity - Vessel.srf_velocity;
                Vessel.SetPosition(updatePostion, true);
                Vessel.ChangeWorldVelocity(velocityOffset);
            }

            Vessel.acceleration = new Vector3d(0, 0, 0);
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

        #endregion
    }
}
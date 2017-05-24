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
        public bool Landed;

        #endregion

        #endregion
        
        #region Private fields
        
        private double PlanetariumDifference { get; set; }
        private const float PlanetariumDifferenceLimit = 3f;
        private int counter = 0;

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
            Landed = msgData.Landed;
        }

        public VesselPositionUpdate(Vessel vessel)
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
                    vessel.ReferenceTransform.rotation.x,
                    vessel.ReferenceTransform.rotation.y,
                    vessel.ReferenceTransform.rotation.z,
                    vessel.ReferenceTransform.rotation.w
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
                Landed = vessel.Landed;
            }
            catch (Exception e)
            {
                Debug.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
                throw e;
            }
        }
        
        public virtual VesselPositionUpdate Clone()
        {
            return this.MemberwiseClone() as VesselPositionUpdate;
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
                setLandedSplashed();
                applyVesselRotation();
                applyVesselPosition();

                //Calculate the srfRelRotation, height from terrain, radar altitude, altitude, and orbit values from the various items set in the above methods.
                //Vessel.UpdatePosVel();
            }
        }

        private void setLandedSplashed()
        {
            Vessel.checkSplashed();
            if (Vessel.packed)
            {
                Vessel.Landed = Landed;
            }
            else
            {
                Vessel.checkLanded();
            }
            
        }

        /// <summary>
        /// Applies common interpolation values
        /// </summary>
        private void applyVesselRotation()
        {
            var targetTransformRot = new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);

            if (SettingsSystem.CurrentSettings.Debug7)
            {
                //We use a tolerance because the coordinate systems do not perfectly match between clients.
                //If the vessel is landed, use a higher tolerance because we want to avoid jitter.  This will cause landed vessels to "jump", but not wiggle and explode.
                double tolerance = Vessel.Landed ? .05 : .01;

                Quaternion rotation = Vessel.vesselTransform.rotation;
                if (Math.Abs(targetTransformRot.w - rotation.w) > tolerance || Math.Abs(targetTransformRot.x - rotation.x) > tolerance || 
                    Math.Abs(targetTransformRot.y - rotation.y) > tolerance || Math.Abs(targetTransformRot.z - rotation.z) > tolerance)
                {
                    //It is necessary to set the position as well, or the parts may jitter if their position is not also set later in the applyVesselPosition method.
                    //It's possible that the threshold for rotation is met, but the threshold for position is not.  In this case, we'll adjust the position of the pieces
                    //by the rotation, but not translate them.
                    Vessel.SetRotation(targetTransformRot, true);
                    Vessel.vesselTransform.rotation = targetTransformRot;
                }
            }
        }


        /// <summary>
        /// Applies surface interpolation values
        /// </summary>
        private void applyVesselPosition()
        {
            //May need to call TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Late, aMethod) to call this at the end of FixedUpdate.
            //It'll happen after the FlightIntegrator code, at least.
            var currentAcc = new Vector3d(0, 0, 0);
            var currentVelocity = new Vector3d(OrbitVelocity[0], OrbitVelocity[1], OrbitVelocity[2]);

            var latitude = LatLonAlt[0];
            var longitude = LatLonAlt[1];
            var altitude = LatLonAlt[2];

            if (!Vessel.LandedOrSplashed)
            {
                //When suborbital, we can just set orbit from velocity and position.  For orbits, this can cause significant jitter.
                if (false && Vessel.altitude < 25000)
                {
                    Vector3d updatePosition = Body.GetWorldSurfacePosition(latitude, longitude, altitude);
                    if (SettingsSystem.CurrentSettings.PositionFudgeEnable)
                    {
                        //Use the average velocity to determine the new position --- Displacement = v0*t + 1/2at^2.
                        var positionFudge = (currentVelocity * PlanetariumDifference);
                        updatePosition += positionFudge;
                    }

                    Vector3d orbitalPos = updatePosition - Vessel.mainBody.position;
                    Vector3d orbitalVel = currentVelocity;
                    Vessel.orbitDriver.orbit.UpdateFromStateVectors(orbitalPos.xzy, orbitalVel.xzy, Body, Planetarium.GetUniversalTime());
                    Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
                    Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel.xzy;

                    
                }
                else
                {
                    var targetOrbit = new Orbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3], Orbit[4], Orbit[5], Orbit[6], Body);

                    if (true || Vessel.packed)
                    {
                        //The OrbitDriver update call will set the vessel position on the next fixed update
                        CopyOrbit(targetOrbit, Vessel.orbitDriver.orbit);
                        Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
                        Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel.xzy;
                    }
                }
            }
            else
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

                Vector3d position = Body.GetWorldSurfacePosition(latitude, longitude, altitude);
                Vector3 currentPosition = Vessel.vesselTransform.position;
                double cur_x = currentPosition.x;
                double cur_y = currentPosition.y;
                double cur_z = currentPosition.z;

                //We use a tolerance because the coordinate systems do not perfectly match between clients.
                //If the vessel is landed, use a higher tolerance because we want to avoid jitter.  This will cause landed vessels to "jump", but not wiggle and explode.
                double tolerance = Vessel.Landed ? .1 : .01;

                if (Math.Abs(position.x - cur_x) > tolerance || Math.Abs(position.y - cur_y) > tolerance || Math.Abs(position.z - cur_z) > tolerance)
                {
                    Vessel.SetPosition(position);
                }
            }

            //Set the velocity on each part and its rigidbody (if it exists)
            //Vector3d vesselUniverseVelocity = (Vessel.orbit.GetVel() - (!Vessel.orbit.referenceBody.inverseRotation ? Vector3d.zero : Vessel.orbit.referenceBody.getRFrmVel(Vessel.vesselTransform.position)));
            //Vector3d vel = vesselUniverseVelocity - Krakensbane.GetFrameVelocity();
            int numParts = Vessel.Parts.Count;
            for (int i = 0; i < numParts; i++)
            {
                Part item = this.Vessel.parts[i];
                item.ResumeVelocity();

                //This is based on the behavior of Part.ResumeVelocity()
                //item.vel = Vector3.zero;
                //if (!Vessel.LandedOrSplashed)
                //{
                //    Vector3 partUniverseVelocity = (item.orbit.GetVel() - (!item.orbit.referenceBody.inverseRotation ? Vector3d.zero : item.orbit.referenceBody.getRFrmVel(item.partTransform.position)));
                //    Vector3 newVelocity = partUniverseVelocity - Krakensbane.GetFrameVelocity();
                //    item.vel = newVelocity;
                //}
                //if (item.rb != null)
                //{
                //    item.rb.velocity = item.vel;
                //}
            }
            Vessel.angularVelocityD = Vector3d.zero;
            Vessel.angularVelocity = Vector3.zero;

            counter++;
            if (Vessel.altitude > 25000 || counter > 75)
            {
                counter = 0;
                //Vessel.orbitDriver.updateFromParameters();
            }

            if (Vessel.loaded && !Vessel.packed)
            {
                Vessel.GetHeightFromTerrain();
            }
        }
        //Need to copy the control states for the vessels
        //Vessel.ctrlState.Neutralize();

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
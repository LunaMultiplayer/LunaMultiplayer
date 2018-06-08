using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using LunaCommon;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionUpdate
    {
        private double MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds * 10
                : double.MaxValue;

        #region Fields

        private Vessel _vessel;
        public Vessel Vessel
        {
            get
            {
                if (_vessel == null)
                    _vessel = FlightGlobals.FindVessel(VesselId);
                return _vessel;
            }
        }

        public CelestialBody Body => GetBody(BodyIndex);
        public CelestialBody LerpBody => LerpPercentage < 0.5 ? GetBody(BodyIndex) : GetBody(Target.BodyIndex);

        public VesselPositionUpdate Target { get; set; }

        private bool CurrentlySpectatingThisVessel => VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id == VesselId;

        #region Message Fields

        public Guid VesselId { get; set; }
        public int BodyIndex { get; set; }
        public bool Landed { get; set; }
        public bool Splashed { get; set; }
        public double[] LatLonAlt { get; set; } = new double[3];
        public double[] NormalVector { get; set; } = new double[3];
        public double[] Velocity { get; set; } = new double[3];
        public double[] Orbit { get; set; } = new double[8];
        public float[] SrfRelRotation { get; set; } = new float[4];
        public float HeightFromTerrain { get; set; }
        public double GameTimeStamp { get; set; }
        public int SubspaceId { get; set; }
        public bool HackingGravity { get; set; }

        #endregion

        #region Vessel position information fields

        public Quaternion SurfaceRelRotation => new Quaternion(SrfRelRotation[0], SrfRelRotation[1], SrfRelRotation[2], SrfRelRotation[3]);
        public Vector3 Normal => new Vector3d(NormalVector[0], NormalVector[1], NormalVector[2]);
        public Vector3d VelocityVector => new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
        public Orbit KspOrbit { get; set; }

        #endregion

        #region Interpolation fields

        public double TimeDifference { get; private set; }
        public double ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;

        public double RawInterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp, 0, MaxInterpolationDuration);
        public double InterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp + ExtraInterpolationTime, 0, MaxInterpolationDuration);

        private float _lerpPercentage = 1;
        public float LerpPercentage
        {
            get => SettingsSystem.CurrentSettings.PositionInterpolation ? _lerpPercentage : 1;
            set => _lerpPercentage = value;
        }

        public double LerpTime { get; set; }

        #endregion

        #endregion

        #region Constructor

        public VesselPositionUpdate() { }

        public VesselPositionUpdate(VesselPositionMsgData msgData)
        {
            VesselId = msgData.VesselId;
            BodyIndex = msgData.BodyIndex;
            SubspaceId = msgData.SubspaceId;
            HeightFromTerrain = msgData.HeightFromTerrain;
            Landed = msgData.Landed;
            Splashed = msgData.Splashed;
            GameTimeStamp = msgData.GameTime;
            HackingGravity = msgData.HackingGravity;

            Array.Copy(msgData.SrfRelRotation, SrfRelRotation, 4);
            Array.Copy(msgData.Velocity, Velocity, 3);
            Array.Copy(msgData.LatLonAlt, LatLonAlt, 3);
            Array.Copy(msgData.NormalVector, NormalVector, 3);
            Array.Copy(msgData.Orbit, Orbit, 8);
        }

        public void CopyFrom(VesselPositionUpdate update)
        {
            VesselId = update.VesselId;
            BodyIndex = update.BodyIndex;
            SubspaceId = update.SubspaceId;
            HeightFromTerrain = update.HeightFromTerrain;
            Landed = update.Landed;
            Splashed = update.Splashed;
            GameTimeStamp = update.GameTimeStamp;
            HackingGravity = update.HackingGravity;

            Array.Copy(update.SrfRelRotation, SrfRelRotation, 4);
            Array.Copy(update.Velocity, Velocity, 3);
            Array.Copy(update.LatLonAlt, LatLonAlt, 3);
            Array.Copy(update.NormalVector, NormalVector, 3);
            Array.Copy(update.Orbit, Orbit, 8);
        }

        #endregion

        #region Main method

        /// <summary>
        /// Call this method to apply a vessel update using interpolation
        /// </summary>
        public void ApplyInterpolatedVesselUpdate()
        {
            if (Vessel == null || Vessel.precalc == null || Vessel.state == Vessel.State.DEAD || Body == null)
            {
                return;
            }

            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == VesselId)
            {
                //Do not apply position updates to our OWN controlled vessel
                return;
            }

            if (InterpolationFinished && VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(VesselId, out var queue) && queue.TryDequeue(out var targetUpdate))
            {
                if (Target == null) //This is the case of first iteration
                    GameTimeStamp = targetUpdate.GameTimeStamp - TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds;

                ProcessRestart();
                LerpPercentage = 0;

                if (Target != null)
                {
                    Target.CopyFrom(targetUpdate);
                    VesselPositionSystem.TargetVesselUpdateQueue[VesselId].Recycle(targetUpdate);
                }
                else
                {
                    Target = targetUpdate;
                }

                AdjustExtraInterpolationTimes();

                KspOrbit = new Orbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3], Orbit[4], Orbit[5], Orbit[6], Body);
                Target.KspOrbit = new Orbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3], Target.Orbit[4], Target.Orbit[5], Target.Orbit[6], Target.Body);

                UpdateProtoVesselValues();
            }

            if (Target == null) return;
            if (LerpPercentage > 1)
            {
                LunaLog.LogWarning("No messages to interpolate to! Increase the interpolation offset!");
            }

            try
            {
                ApplyInterpolations();
            }
            catch (Exception e)
            {
                LunaLog.LogError($"ApplyInterpolations: {e}");
            }
            finally
            {
                LerpPercentage += (float)(Time.fixedDeltaTime / InterpolationDuration);
            }
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced
        /// </summary>
        private void AdjustExtraInterpolationTimes()
        {
            TimeDifference = TimeSyncerSystem.UniversalTime - GameTimeStamp;
            if (SubspaceId == -1)
            {
                //While warping we only fix the interpolation if we are LAGGING behind the updates
                //We never fix it if the packet that we received is very advanced in time
                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffsetSeconds ? -1 : 0) * GetInterpolationFixFactor();
            }
            else 
            {
                //IN past or same subspaces we want to be SettingsSystem.CurrentSettings.InterpolationOffset seconds BEHIND the player position
                if (WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                {
                    //The subspace is in the past so add the difference seconds to normalize it
                    var timeToAdd = Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference += timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffsetSeconds ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        /// <summary>
        /// This gives the fix factor. It scales up or down depending on the error we have
        /// </summary>
        private double GetInterpolationFixFactor()
        {
            var error = TimeSpan.FromSeconds(Math.Abs(TimeDifference)).TotalMilliseconds;

            //Do not use less than 0.25 as otherwise it won't fix it.
            if (error <= 500)
            {
                return RawInterpolationDuration * 0.25;
            }

            if (error <= 1000)
            {
                return RawInterpolationDuration * 0.60;
            }

            if (error <= 5000)
            {
                return RawInterpolationDuration;
            }

            return RawInterpolationDuration * 2;
        }

        #endregion

        #region Private

        private void UpdateProtoVesselValues()
        {
            Vessel.protoVessel.latitude = Target.LatLonAlt[0];
            Vessel.protoVessel.longitude = Target.LatLonAlt[1];
            Vessel.protoVessel.altitude = Target.LatLonAlt[2];
            Vessel.protoVessel.height = Target.HeightFromTerrain;

            Vessel.protoVessel.normal[0] = Target.Normal[0];
            Vessel.protoVessel.normal[1] = Target.Normal[1];
            Vessel.protoVessel.normal[2] = Target.Normal[2];

            Vessel.protoVessel.rotation[0] = Target.SrfRelRotation[0];
            Vessel.protoVessel.rotation[1] = Target.SrfRelRotation[1];
            Vessel.protoVessel.rotation[2] = Target.SrfRelRotation[2];
            Vessel.protoVessel.rotation[3] = Target.SrfRelRotation[3];

            Vessel.protoVessel.orbitSnapShot.inclination = Target.Orbit[0];
            Vessel.protoVessel.orbitSnapShot.eccentricity = Target.Orbit[1];
            Vessel.protoVessel.orbitSnapShot.semiMajorAxis = Target.Orbit[2];
            Vessel.protoVessel.orbitSnapShot.LAN = Target.Orbit[3];
            Vessel.protoVessel.orbitSnapShot.argOfPeriapsis = Target.Orbit[4];
            Vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = Target.Orbit[5];
            Vessel.protoVessel.orbitSnapShot.epoch = Target.Orbit[6];
            Vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = (int)Target.Orbit[7];
        }

        private void ApplyInterpolations()
        {
            if (Vessel.isEVA && Vessel.loaded)
            {
                ApplyPositionsToEva();
                return;
            }

            ApplyOrbitInterpolation();

            //Do not use CoM. It's not needed and it generate issues when you patch the protovessel with it as it generate weird commnet lines
            //It's important to set the static pressure as otherwise the vessel situation is not updated correctly when
            //Vessel.updateSituation() is called in the Vessel.LateUpdate(). Same applies for landed and splashed
            Vessel.staticPressurekPa = FlightGlobals.getStaticPressure(Target.LatLonAlt[2], Body);
            Vessel.heightFromTerrain = Target.HeightFromTerrain;

            if (!Vessel.loaded)
            {
                //DO NOT lerp the latlonalt as otherwise if you are in orbit you will see landed vessels in the map view with weird jittering
                Vessel.latitude = Target.LatLonAlt[0];
                Vessel.longitude = Target.LatLonAlt[1];
                Vessel.altitude = Target.LatLonAlt[2];
                Vessel.orbitDriver.updateFromParameters();

                if (Vessel.LandedOrSplashed)
                    Vessel.SetPosition(Target.Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
            }
            else
            {
                ApplyInterpolationsToLoadedVessel();
            }
        }


        private void ApplyInterpolationsToLoadedVessel()
        {
            var currentSurfaceRelRotation = Quaternion.SlerpUnclamped(SurfaceRelRotation, Target.SurfaceRelRotation, LerpPercentage);
            var curVelocity = VectorUtil.LerpUnclamped(VelocityVector, Target.VelocityVector, LerpPercentage);

            //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped.
            Vessel.SetWorldVelocity(curVelocity);
            Vessel.velocityD = curVelocity;

            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            Vessel.srfRelRotation = currentSurfaceRelRotation;
            Vessel.SetRotation((Quaternion)Body.rotation * currentSurfaceRelRotation, true);

            Vessel.Landed = LerpPercentage < 0.5 ? Landed : Target.Landed;
            Vessel.Splashed = LerpPercentage < 0.5 ? Splashed : Target.Splashed;

            //Set the position of the vessel based on the orbital parameters
            //Don't call this method as we are replaying orbits from back an older time!
            Vessel.orbitDriver.updateFromParameters();

            if (Vessel.LandedOrSplashed)
            {
                Vessel.latitude = LunaMath.LerpUnclamped(LatLonAlt[0], Target.LatLonAlt[0], LerpPercentage);
                Vessel.longitude = LunaMath.LerpUnclamped(LatLonAlt[1], Target.LatLonAlt[1], LerpPercentage);
                Vessel.altitude = LunaMath.LerpUnclamped(LatLonAlt[2], Target.LatLonAlt[2], LerpPercentage);

                Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
            }

            foreach (var part in Vessel.Parts)
                part.ResumeVelocity();

            if (HackingGravity)
            {
                if (Vessel.LandedOrSplashed || Vessel.situation <= Vessel.Situations.FLYING)
                    Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
            }

            if (CurrentlySpectatingThisVessel)
            {
                Vessel.UpdatePosVel();
                Vessel.precalc.CalculatePhysicsStats(); //This will update the localCom and other variables of the vessel
            }
        }

        /// <summary>
        /// Kerbals positioning is quite messy....
        /// </summary>
        private void ApplyPositionsToEva()
        {
            Vessel.latitude = LunaMath.LerpUnclamped(LatLonAlt[0], Target.LatLonAlt[0], LerpPercentage);
            Vessel.longitude = LunaMath.LerpUnclamped(LatLonAlt[1], Target.LatLonAlt[1], LerpPercentage);
            Vessel.altitude = LunaMath.LerpUnclamped(LatLonAlt[2], Target.LatLonAlt[2], LerpPercentage);

            Vessel.Landed = LerpPercentage < 0.5 ? Landed : Target.Landed;
            Vessel.Splashed = LerpPercentage < 0.5 ? Splashed : Target.Splashed;

            var currentSurfaceRelRotation = Quaternion.SlerpUnclamped(SurfaceRelRotation, Target.SurfaceRelRotation, LerpPercentage);
            Vessel.SetRotation((Quaternion)Vessel.mainBody.rotation * currentSurfaceRelRotation, true);
            Vessel.srfRelRotation = currentSurfaceRelRotation;

            ApplyOrbitInterpolation();
            //Don't call this method as we are replaying orbits from back an older time!
            //Vessel.orbitDriver.updateFromParameters();

            //We don't do the surface positioning as with vessels because kerbals don't walk at high speeds and with this code it will be enough ;)
            if (Vessel.LandedOrSplashed || Vessel.situation <= Vessel.Situations.FLYING)
                Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
        }

        private void ApplyOrbitInterpolation()
        {
            var startTime = KspOrbit.epoch;
            var targetTime = Target.KspOrbit.epoch;

            var currentPos = KspOrbit.getRelativePositionAtUT(startTime);
            var targetPos = Target.KspOrbit.getRelativePositionAtUT(targetTime);

            var currentVel = KspOrbit.getOrbitalVelocityAtUT(startTime) + KspOrbit.referenceBody.GetFrameVelAtUT(startTime) - Body.GetFrameVelAtUT(startTime);
            var targetVel = Target.KspOrbit.getOrbitalVelocityAtUT(targetTime) + Target.KspOrbit.referenceBody.GetFrameVelAtUT(targetTime) - Target.Body.GetFrameVelAtUT(targetTime);

            var lerpedPos = VectorUtil.LerpUnclamped(currentPos, targetPos, LerpPercentage);
            var lerpedVel = VectorUtil.LerpUnclamped(currentVel, targetVel, LerpPercentage);

            LerpTime = LunaMath.LerpUnclamped(startTime, targetTime, LerpPercentage);
            Vessel.orbitDriver.orbit.UpdateFromStateVectors(lerpedPos, lerpedVel, LerpBody, LerpTime);
        }

        /// <summary>
        /// Here we apply the CURRENT vessel position to this update.
        /// </summary>
        private void ProcessRestart()
        {
            if (Target != null)
            {
                GameTimeStamp = Target.GameTimeStamp;
                BodyIndex = Target.BodyIndex;
                Landed = Target.Landed;
                Splashed = Target.Splashed;
                SubspaceId = Target.SubspaceId;

                Array.Copy(Target.SrfRelRotation, SrfRelRotation, 4);
                Array.Copy(Target.Velocity, Velocity, 3);
                Array.Copy(Target.LatLonAlt, LatLonAlt, 3);
                Array.Copy(Target.NormalVector, NormalVector, 3);
                Array.Copy(Target.Orbit, Orbit, 8);

                HeightFromTerrain = Target.HeightFromTerrain;
                HackingGravity = Target.HackingGravity;
            }
            else
            {
                BodyIndex = Vessel.mainBody.flightGlobalsIndex;
                Landed = Vessel.Landed;
                Splashed = Vessel.Splashed;

                SrfRelRotation[0] = Vessel.srfRelRotation.x;
                SrfRelRotation[1] = Vessel.srfRelRotation.y;
                SrfRelRotation[2] = Vessel.srfRelRotation.z;
                SrfRelRotation[3] = Vessel.srfRelRotation.w;

                Vector3d srfVel = Quaternion.Inverse(Body.bodyTransform.rotation) * Vessel.srf_velocity;
                Velocity[0] = srfVel.x;
                Velocity[1] = srfVel.y;
                Velocity[2] = srfVel.z;

                LatLonAlt[0] = Vessel.latitude;
                LatLonAlt[1] = Vessel.longitude;
                LatLonAlt[2] = Vessel.altitude;

                NormalVector[0] = Vessel.terrainNormal.x;
                NormalVector[1] = Vessel.terrainNormal.y;
                NormalVector[2] = Vessel.terrainNormal.z;

                Orbit[0] = Vessel.orbit.inclination;
                Orbit[1] = Vessel.orbit.eccentricity;
                Orbit[2] = Vessel.orbit.semiMajorAxis;
                Orbit[3] = Vessel.orbit.LAN;
                Orbit[4] = Vessel.orbit.argumentOfPeriapsis;
                Orbit[5] = Vessel.orbit.meanAnomalyAtEpoch;
                Orbit[6] = Vessel.orbit.epoch;
                Orbit[7] = Vessel.orbit.referenceBody.flightGlobalsIndex;

                HeightFromTerrain = Vessel.heightFromTerrain;
            }
        }

        #region Helper methods

        private static CelestialBody GetBody(int bodyIndex)
        {
            try
            {
                return FlightGlobals.Bodies[bodyIndex];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void UpdateFromParameters(OrbitDriver driver, double time)
        {
            driver.orbit.UpdateFromUT(time);
            driver.pos = driver.orbit.pos;
            driver.vel = driver.orbit.vel;
            driver.pos.Swizzle();
            driver.vel.Swizzle();
            if (double.IsNaN(driver.pos.x))
            {
                driver.vessel?.Unload();
                UnityEngine.Object.Destroy(driver.vessel.gameObject);
                return;
            }
            if (driver.reverse)
            {
                Vector3d vector3d;
                if (!driver.celestialBody)
                {
                    vector3d = driver.driverTransform.position;
                }
                else
                {
                    vector3d = driver.celestialBody.position;
                }
                driver.referenceBody.position = vector3d - driver.pos;
            }
            else if (driver.vessel)
            {
                var vector3d1 = driver.driverTransform.rotation * driver.vessel.localCoM;
                driver.vessel.SetPosition((driver.referenceBody.position + driver.pos) - vector3d1);
            }
            else if (!driver.celestialBody)
            {
                driver.driverTransform.position = driver.referenceBody.position + driver.pos;
            }
            else
            {
                driver.celestialBody.position = driver.referenceBody.position + driver.pos;
            }
        }

        #endregion

        #endregion
    }
}

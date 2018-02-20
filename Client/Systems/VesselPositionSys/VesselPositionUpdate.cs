using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
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
        #region Fields

        public Vessel Vessel { get; set; }
        public CelestialBody Body => LerpPercentage < 0.5 ? GetBody(BodyIndex) : GetBody(Target.BodyIndex);

        private VesselPositionUpdate _target;
        public VesselPositionUpdate Target
        {
            get
            {
                if (_target == null)
                    VesselPositionSystem.TargetVesselUpdate.TryGetValue(VesselId, out _target);
                return _target;
            }
        }
        
        #region Message Fields

        public Guid VesselId { get; set; }
        public int BodyIndex { get; set; }
        public double[] LatLonAlt { get; set; } = new double[3];
        public double[] NormalVector { get; set; } = new double[3];
        public double[] Com { get; set; } = new double[3];
        public double[] ComD { get; set; } = new double[3];
        public double[] TransformPosition { get; set; } = new double[3];
        public double[] Velocity { get; set; } = new double[3];
        public double[] OrbitPos { get; set; } = new double[3];
        public double[] OrbitVel { get; set; } = new double[3];
        public double[] Orbit { get; set; } = new double[8];
        public float[] SrfRelRotation { get; set; } = new float[4];

        public float HeightFromTerrain;
        public long TimeStamp;

        #endregion

        #region Vessel position information fields

        public Quaternion SurfaceRelRotation => new Quaternion(SrfRelRotation[0], SrfRelRotation[1], SrfRelRotation[2], SrfRelRotation[3]);
        public Vector3d TransformPos => new Vector3d(TransformPosition[0], TransformPosition[1], TransformPosition[2]);
        public Vector3 CoM => new Vector3d(Com[0], Com[1], Com[2]);
        public Vector3 CoMd => new Vector3d(ComD[0], ComD[1], ComD[2]);
        public Vector3 Normal => new Vector3d(NormalVector[0], NormalVector[1], NormalVector[2]);
        public Vector3d VelocityVector => new Vector3d(Velocity[0], Velocity[1], Velocity[2]);
        public Vector3d OrbitPosVec => new Vector3d(OrbitPos[0], OrbitPos[1], OrbitPos[2]);
        public Vector3d OrbitVelVec => new Vector3d(OrbitVel[0], OrbitVel[1], OrbitVel[2]);

        #endregion

        #region Interpolation fields

        public bool InterpolationStarted { get; set; }
        public bool RestartRequested { get; set; }
        public bool InterpolationFinished { get; set; }
        public float InterpolationDuration => (float)TimeSpan.FromTicks(Target.TimeStamp - TimeStamp).TotalSeconds;

        private float _lerpPercentage;
        public float LerpPercentage
        {
            get => !SettingsSystem.CurrentSettings.InterpolationEnabled ? 1 : _lerpPercentage;
            set => _lerpPercentage = value;
        }

        #endregion

        #endregion

        #region Main method

        /// <summary>
        /// Call this method to reset a vessel position update so you can reuse it
        /// </summary>
        public void ResetFields()
        {
            Vessel = FlightGlobals.Vessels.Find(v => v.id == VesselId);

            LerpPercentage = 0;
            InterpolationStarted = false;
            InterpolationFinished = false;
            _target = null;
        }

        /// <summary>
        /// Call this method to apply the current vessel position to this update. Usefull for interpolating
        /// </summary>
        public void Restart()
        {
            Vessel = FlightGlobals.Vessels.Find(v => v.id == VesselId);

            RestartRequested = true;
            TimeStamp = Target?.TimeStamp ?? TimeStamp;
            ResetFields();
        }

        /// <summary>
        /// Call this method to apply a vessel update either by interpolation or just by raw positioning the vessel
        /// </summary>
        public void ApplyVesselUpdate()
        {
            Vessel = FlightGlobals.Vessels.Find(v => v.id == VesselId);

            if (Body == null || Vessel == null || Vessel.precalc == null || Vessel.state == Vessel.State.DEAD || Target == null ||
                FlightGlobals.ActiveVessel?.id == VesselId && !VesselCommon.IsSpectating)
            {
                VesselPositionSystem.VesselsToRemove.Enqueue(VesselId);
                return;
            }

            if (InterpolationFinished) return;
            if (!InterpolationStarted)
            {
                InterpolationStarted = true;
                UpdateProtoVesselValues();
            }

            if (RestartRequested)
            {
                ProcessRestart();
            }

            try
            {
                if (LerpPercentage <= 1)
                {
                    ApplyInterpolations(LerpPercentage);
                    LerpPercentage += Time.fixedDeltaTime / InterpolationDuration;
                }
                else
                {
                    InterpolationFinished = true;
                }
            }
            catch
            {
                // ignored
            }
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

            Vessel.protoVessel.CoM[0] = Target.CoM[0];
            Vessel.protoVessel.CoM[1] = Target.CoM[1];
            Vessel.protoVessel.CoM[2] = Target.CoM[2];

            Vessel.protoVessel.orbitSnapShot.inclination = Target.Orbit[0];
            Vessel.protoVessel.orbitSnapShot.eccentricity = Target.Orbit[1];
            Vessel.protoVessel.orbitSnapShot.semiMajorAxis = Target.Orbit[2];
            Vessel.protoVessel.orbitSnapShot.LAN = Target.Orbit[3];
            Vessel.protoVessel.orbitSnapShot.argOfPeriapsis = Target.Orbit[4];
            Vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = Target.Orbit[5];
            Vessel.protoVessel.orbitSnapShot.epoch = Target.Orbit[6];
            Vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = (int)Target.Orbit[7];
        }

        private void OldApplyInterpolationsToLoadedVessel(float lerpPercentage)
        {
            var currentSurfaceRelRotation = Quaternion.Lerp(SurfaceRelRotation, Target.SurfaceRelRotation, lerpPercentage);
            var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage);

            //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped
            Vessel.SetWorldVelocity(curVelocity);
            Vessel.velocityD = curVelocity;

            //Apply rotation
            Vessel.SetRotation((Quaternion)Vessel.mainBody.rotation * currentSurfaceRelRotation, true);
            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            Vessel.srfRelRotation = currentSurfaceRelRotation;

            //If you do Vessel.ReferenceTransform.position = curPosition 
            //then in orbit vessels crash when they get unpacked and also vessels go inside terrain randomly
            //that is the reason why we pack vessels at close distance when landed...
            switch (Vessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], lerpPercentage);
                    Vessel.longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], lerpPercentage);
                    Vessel.altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], lerpPercentage);
                    break;
            }

            //Vessel.heightFromTerrain = Target.Height; //NO need to set the height from terrain, not even in flying
            Vessel.orbitDriver.updateFromParameters();
            foreach (var part in Vessel.Parts)
                part.ResumeVelocity();
        }

        private void DarkApplyInterpolationsToLoadedVessel(float lerpPercentage)
        {
            //Get worldspace rotation quaternion and velocity vectors.
            var currentSurfaceRelRotation = Quaternion.Lerp(SurfaceRelRotation, Target.SurfaceRelRotation, lerpPercentage);
            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            Vessel.srfRelRotation = currentSurfaceRelRotation;
            var curVelocity = (Vessel.mainBody.bodyTransform.rotation * Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage)) - Krakensbane.GetFrameVelocity();
            //Use lat/long/alt/vel under 1km
            var useOrbitDriver = Target.LatLonAlt[2] > 1000;

            //DO NOT lerp the latlonalt as otherwise if you are in 
            //orbit you will see landed vessels in the map view with weird jittering
            if (useOrbitDriver)
            {
                Vessel.latitude = Target.LatLonAlt[0];
                Vessel.longitude = Target.LatLonAlt[1];
                Vessel.altitude = Target.LatLonAlt[2];
            }
            else
            {
                Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], lerpPercentage);
                Vessel.longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], lerpPercentage);
                Vessel.altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], lerpPercentage);
            }

            var surfacePos = Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude);
            if (useOrbitDriver || (Vessel.packed))
            {
                Vessel.orbitDriver.updateFromParameters();
                if (!Vessel.packed)
                {
                    var fudgeVel = Body.inverseRotation ? Body.getRFrmVelOrbit(Vessel.orbitDriver.orbit) : Vector3d.zero;
                    Vessel.SetWorldVelocity(Vessel.orbitDriver.orbit.vel.xzy - fudgeVel - Krakensbane.GetFrameVelocity());
                }
            }
            else
            {
                Vessel.SetPosition(surfacePos);
                Vessel.SetWorldVelocity(curVelocity);
                //This sets obt_vel so acceleration can be fixed in Vessel.UpdateAcceleration
                if (Vessel.orbitDriver.updateMode != OrbitDriver.UpdateMode.UPDATE)
                {
                    Vessel.orbitDriver.UpdateOrbit();
                }
            }


            //Apply rotation (also resets posistion)
            Vessel.SetRotation((Quaternion)Vessel.mainBody.rotation * currentSurfaceRelRotation, true);
            //Not sure if we need to touch reference transform?
            //If you do Vessel.ReferenceTransform.position = curPosition 
            //then in orbit vessels crash when they get unpacked and also vessels go inside terrain randomly
            //that is the reason why we pack vessels at close distance when landed...

            //Vessel.heightFromTerrain = Target.Height; //NO need to set the height from terrain, not even in flying
            Vessel.UpdatePosVel();
            if (Vessel.orbitDriver.updateMode != OrbitDriver.UpdateMode.UPDATE)
            {
                Vessel.UpdateAcceleration(1 / TimeWarp.fixedDeltaTime, false);
            }
        }

        private void MixedApplyInterpolationsToLoadedVessel(float lerpPercentage)
        {
            var currentSurfaceRelRotation = Quaternion.Lerp(SurfaceRelRotation, Target.SurfaceRelRotation, lerpPercentage);
            var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage);

            //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped
            Vessel.SetWorldVelocity(curVelocity);
            Vessel.velocityD = curVelocity;

            //Apply rotation
            Vessel.SetRotation((Quaternion)Vessel.mainBody.rotation * currentSurfaceRelRotation, true);
            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            Vessel.srfRelRotation = currentSurfaceRelRotation;

            //Vessel.heightFromTerrain = Target.Height; //NO need to set the height from terrain, not even in flying
            Vessel.orbitDriver.updateFromParameters();

            //If you do Vessel.ReferenceTransform.position = curPosition 
            //then in orbit vessels crash when they get unpacked and also vessels go inside terrain randomly
            //that is the reason why we pack vessels at close distance when landed...
            switch (Vessel.situation)
            {
                case Vessel.Situations.LANDED:
                case Vessel.Situations.SPLASHED:
                    Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], lerpPercentage);
                    Vessel.longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], lerpPercentage);
                    Vessel.altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], lerpPercentage);
                    Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
                    Vessel.SetWorldVelocity(curVelocity);
                    break;
            }

            foreach (var part in Vessel.Parts)
                part.ResumeVelocity();

            if (VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id == VesselId)
            {
                Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
            }
        }

        private void ApplyInterpolations(float lerpPercentage)
        {
            ApplyOrbitInterpolation(lerpPercentage);

            //TODO: Is CoM and terrainNormal really needed?
            Vessel.CoM = Vector3.Lerp(CoM, Target.CoM, lerpPercentage);
            Vessel.CoMD = Vector3d.Lerp(CoMd, Target.CoMd, lerpPercentage);
            Vessel.terrainNormal = Vector3.Lerp(Normal, Target.Normal, lerpPercentage);

            //It's important to set the static pressure as otherwise the vessel situation is not updated correctly when
            //Vessel.updateSituation() is called in the Vessel.LateUpdate(). Same applies for landed and splashed
            Vessel.staticPressurekPa = FlightGlobals.getStaticPressure(Target.LatLonAlt[2], Vessel.mainBody);
            Vessel.heightFromTerrain = HeightFromTerrain;

            if (!Vessel.loaded)
            {
                //DO NOT lerp the latlonalt as otherwise if you are in 
                //orbit you will see landed vessels in the map view with weird jittering
                Vessel.latitude = Target.LatLonAlt[0];
                Vessel.longitude = Target.LatLonAlt[1];
                Vessel.altitude = Target.LatLonAlt[2];
                Vessel.orbitDriver.updateFromParameters();
            }
            else
            {
                if (SettingsSystem.CurrentSettings.PositionSystem > 2)
                {
                    LunaLog.LogWarning("Position system setting out of range.  Setting to default...");
                    SettingsSystem.CurrentSettings.PositionSystem = 2;
                }

                switch (SettingsSystem.CurrentSettings.PositionSystem)
                {
                    case 0:
                        DarkApplyInterpolationsToLoadedVessel(lerpPercentage);
                        break;
                    case 1:
                        OldApplyInterpolationsToLoadedVessel(lerpPercentage);
                        break;
                    case 2:
                        MixedApplyInterpolationsToLoadedVessel(lerpPercentage);
                        break;
                }
            }
        }

        private void ApplyOrbitInterpolation(float lerpPercentage)
        {
            //Avoid lerping longitude of ascending node / arg of periapsis values when the inclination / eccentricity is close to 0 as it generate rounding errors!
            if (Math.Abs(Orbit[0]) < 0.001 || Math.Abs(Orbit[1]) < 0.001)
                lerpPercentage = 1;

            var inclination = LerpAngle(Orbit[0], Target.Orbit[0], lerpPercentage);
            var eccentricity = Lerp(Orbit[1], Target.Orbit[1], lerpPercentage);
            var semiMajorAxis = Lerp(Orbit[2], Target.Orbit[2], lerpPercentage);
            var lan = LerpAngle(Orbit[3], Target.Orbit[3], lerpPercentage);
            var argPeriapsis = LerpAngle(Orbit[4], Target.Orbit[4], lerpPercentage);
            var meanAnomalyEpoch = LerpAngle(Orbit[5], Target.Orbit[5], lerpPercentage);
            var epoch = Lerp(Orbit[6], Target.Orbit[6], lerpPercentage);

            //Do not set the body explicitely!! Don't do ---> Vessel.orbitDriver.referenceBody = Body;
            Vessel.orbitDriver.orbit.SetOrbit
            (
                inclination,
                eccentricity,
                semiMajorAxis,
                lan,
                argPeriapsis,
                meanAnomalyEpoch,
                epoch,
                Body
            );

            Vessel.orbitDriver.orbit.pos = Vector3d.Lerp(OrbitPosVec, Target.OrbitPosVec, lerpPercentage);
            Vessel.orbitDriver.orbit.vel = Vector3d.Lerp(OrbitVelVec, Target.OrbitVelVec, lerpPercentage);

            Vessel.orbitDriver.orbit.UpdateFromStateVectors(Vessel.orbitDriver.orbit.pos, Vessel.orbitDriver.orbit.vel, Body, Planetarium.GetUniversalTime());
        }

        /// <summary>
        /// Here we apply the CURRENT vessel position to this update.
        /// </summary>
        private void ProcessRestart()
        {
            RestartRequested = false;
            if (!SettingsSystem.CurrentSettings.InterpolationEnabled)
                return;

            SrfRelRotation[0] = Vessel.srfRelRotation.x;
            SrfRelRotation[1] = Vessel.srfRelRotation.y;
            SrfRelRotation[2] = Vessel.srfRelRotation.z;
            SrfRelRotation[3] = Vessel.srfRelRotation.w;

            TransformPosition[0] = Vessel.ReferenceTransform.position.x;
            TransformPosition[1] = Vessel.ReferenceTransform.position.y;
            TransformPosition[2] = Vessel.ReferenceTransform.position.z;

            Vector3d srfVel = Quaternion.Inverse(Body.bodyTransform.rotation) * Vessel.srf_velocity;
            Velocity[0] = srfVel.x;
            Velocity[1] = srfVel.y;
            Velocity[2] = srfVel.z;

            LatLonAlt[0] = Vessel.latitude;
            LatLonAlt[1] = Vessel.longitude;
            LatLonAlt[2] = Vessel.altitude;

            Com[0] = Vessel.CoM.x;
            Com[1] = Vessel.CoM.y;
            Com[2] = Vessel.CoM.z;
            
            ComD[0] = Vessel.CoMD.x;
            ComD[1] = Vessel.CoMD.y;
            ComD[2] = Vessel.CoMD.z;

            NormalVector[0] = Vessel.terrainNormal.x;
            NormalVector[1] = Vessel.terrainNormal.y;
            NormalVector[2] = Vessel.terrainNormal.z;

            OrbitPos[0] = Vessel.orbit.pos.x;
            OrbitPos[1] = Vessel.orbit.pos.y;
            OrbitPos[2] = Vessel.orbit.pos.z;

            OrbitVel[0] = Vessel.orbit.vel.x;
            OrbitVel[1] = Vessel.orbit.vel.y;
            OrbitVel[2] = Vessel.orbit.vel.z;

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

        #region Helper methods

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        private static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        private static double LerpAngle(double from, double to, float t)
        {
            var single = Repeat(to - from, 360);
            if (single > 180f)
            {
                single -= 360f;
            }
            return from + single * t;
        }

        private static double Repeat(double t, double length)
        {
            return t - Math.Floor(t / length) * length;
        }

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

        #endregion

        #endregion
    }
}
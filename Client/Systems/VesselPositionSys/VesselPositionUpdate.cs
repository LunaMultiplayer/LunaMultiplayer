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

        public Vessel Vessel => FlightGlobals.Vessels.Find(v => v.id == VesselId);

        private CelestialBody _body;
        public CelestialBody Body
        {
            get { return _body ?? (_body = FlightGlobals.Bodies.Find(b => b.flightGlobalsIndex == BodyIndex)); }
            set => _body = value;
        }

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
        public double[] TransformPosition { get; set; } = new double[3];
        public double[] Velocity { get; set; } = new double[3];
        public double[] Orbit { get; set; } = new double[8];
        public float[] SrfRelRotation { get; set; } = new float[4];

        public float HeightFromTerrain;
        public long TimeStamp;

        #endregion

        #region Vessel position information fields

        public Quaternion SurfaceRelRotation => new Quaternion(SrfRelRotation[0], SrfRelRotation[1], SrfRelRotation[2], SrfRelRotation[3]);
        public Vector3d TransformPos => new Vector3d(TransformPosition[0], TransformPosition[1], TransformPosition[2]);
        public Vector3 CoM => new Vector3d(Com[0], Com[1], Com[2]);
        public Vector3 Normal => new Vector3d(NormalVector[0], NormalVector[1], NormalVector[2]);
        public Vector3d VelocityVector => new Vector3d(Velocity[0], Velocity[1], Velocity[2]);

        private Vector3d _position = Vector3d.zero;
        public Vector3d Position
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
            LerpPercentage = 0;
            InterpolationStarted = false;
            InterpolationFinished = false;
            _position = Vector3d.zero;
            _body = null;
            _target = null;
        }

        /// <summary>
        /// Call this method to apply the current vessel position to this update. Usefull for interpolating
        /// </summary>
        public void Restart()
        {
            RestartRequested = true;
            TimeStamp = Target?.TimeStamp ?? TimeStamp;
            ResetFields();
        }

        /// <summary>
        /// Call this method to apply a vessel update either by interpolation or just by raw positioning the vessel
        /// </summary>
        public void ApplyVesselUpdate()
        {
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
            Vessel.protoVessel.latitude = LatLonAlt[0];
            Vessel.protoVessel.longitude = LatLonAlt[1];
            Vessel.protoVessel.altitude = LatLonAlt[2];
            Vessel.protoVessel.height = HeightFromTerrain;

            Vessel.protoVessel.normal[0] = Normal[0];
            Vessel.protoVessel.normal[1] = Normal[1];
            Vessel.protoVessel.normal[2] = Normal[2];

            Vessel.protoVessel.rotation[0] = SrfRelRotation[0];
            Vessel.protoVessel.rotation[1] = SrfRelRotation[1];
            Vessel.protoVessel.rotation[2] = SrfRelRotation[2];
            Vessel.protoVessel.rotation[3] = SrfRelRotation[3];

            Vessel.protoVessel.CoM[0] = CoM[0];
            Vessel.protoVessel.CoM[1] = CoM[1];
            Vessel.protoVessel.CoM[2] = CoM[2];

            Vessel.protoVessel.orbitSnapShot.inclination = Orbit[0];
            Vessel.protoVessel.orbitSnapShot.eccentricity = Orbit[1];
            Vessel.protoVessel.orbitSnapShot.semiMajorAxis = Orbit[2];
            Vessel.protoVessel.orbitSnapShot.LAN = Orbit[3];
            Vessel.protoVessel.orbitSnapShot.argOfPeriapsis = Orbit[4];
            Vessel.protoVessel.orbitSnapShot.meanAnomalyAtEpoch = Orbit[5];
            Vessel.protoVessel.orbitSnapShot.epoch = Orbit[6];
            Vessel.protoVessel.orbitSnapShot.ReferenceBodyIndex = (int)Orbit[7];
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
                    default:
                        LunaLog.LogWarning("Position system setting out of range.  Setting to default...");
                        SettingsSystem.CurrentSettings.PositionSystem = 1;
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

            Vector3d srfVel = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * Vessel.srf_velocity;
            Velocity[0] = srfVel.x;
            Velocity[1] = srfVel.y;
            Velocity[2] = srfVel.z;

            LatLonAlt[0] = Vessel.latitude;
            LatLonAlt[1] = Vessel.longitude;
            LatLonAlt[2] = Vessel.altitude;

            Com[0] = Vessel.CoM.x;
            Com[1] = Vessel.CoM.y;
            Com[2] = Vessel.CoM.z;

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

        #region Helper methods

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        private static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        public static double LerpAngle(double from, double to, float t)
        {
            var single = Repeat(to - from, 360);
            if (single > 180f)
            {
                single -= 360f;
            }
            return from + single * t;
        }

        public static double Repeat(double t, double length)
        {
            return t - Math.Floor(t / length) * length;
        }

        #endregion

        #endregion
    }
}
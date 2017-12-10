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
            get { return _body ?? (_body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName)); }
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
        public string BodyName { get; set; }
        public double[] LatLonAlt { get; set; }
        public double[] NormalVector { get; set; }
        public double[] Com { get; set; }
        public double[] TransformPosition { get; set; }
        public double[] Velocity { get; set; }
        public double[] Orbit { get; set; }
        public float[] SrfRelRotation { get; set; }
        public bool Landed { get; set; }
        public bool Splashed { get; set; }
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
            TimeStamp = Target.TimeStamp;
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
            if (!InterpolationStarted) InterpolationStarted = true;

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

                InterpolationFinished = LerpPercentage >= 1;
            }
            catch
            {
                // ignored
            }
        }

        #endregion

        #region Private

        private void ApplyInterpolations(float lerpPercentage)
        {
            var beforePos = Vessel.orbitDriver.orbit.getPositionAtUT(Planetarium.GetUniversalTime());
            var beforeSpeed = Vessel.orbitDriver.orbit.getOrbitalVelocityAtUT(Planetarium.GetUniversalTime()).xzy;

            Vessel.orbitDriver.orbit.SetOrbit
            (
                //This probably won't work as orbital definitions aren't necessarily a linear function, and lerping applies a linear interpolation
                Lerp(Orbit[0], Target.Orbit[0], lerpPercentage),
                Lerp(Orbit[1], Target.Orbit[1], lerpPercentage),
                Lerp(Orbit[2], Target.Orbit[2], lerpPercentage),
                Lerp(Orbit[3], Target.Orbit[3], lerpPercentage),
                Lerp(Orbit[4], Target.Orbit[4], lerpPercentage),
                Lerp(Orbit[5], Target.Orbit[5], lerpPercentage),
                Lerp(Orbit[6], Target.Orbit[6], lerpPercentage),
                Body
            );

            //TODO: Is CoM and terrainNormal really needed?
            Vessel.CoM = Vector3.Lerp(CoM, Target.CoM, lerpPercentage);
            Vessel.terrainNormal = Vector3.Lerp(Normal, Target.Normal, lerpPercentage);

            //It's important to set the static pressure as otherwise the vessel situation is not updated correctly when
            //Vessel.updateSituation() is called in the Vessel.LateUpdate(). Same applies for landed and splashed
            Vessel.staticPressurekPa = FlightGlobals.getStaticPressure(Target.LatLonAlt[2], Vessel.mainBody);
            Vessel.Landed = Landed;
            Vessel.Splashed = Splashed;

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
                var curRotation = Quaternion.Lerp(SurfaceRelRotation, Target.SurfaceRelRotation, lerpPercentage);
                var curPosition = Vector3d.Lerp(TransformPos, Target.TransformPos, lerpPercentage);
                var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage);

                //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped
                Vessel.SetWorldVelocity(curVelocity);
                Vessel.velocityD = curVelocity;

                //Apply rotation
                Quaternion mainBodyRotation = Vessel.mainBody.rotation;
                Vessel.SetRotation(mainBodyRotation * curRotation, true);
                //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
                Vessel.srfRelRotation = curRotation;

                //If you do Vessel.ReferenceTransform.position = curPosition 
                //then in orbit vessels crash when they get unpacked and also vessels go inside terrain randomly
                //that is the reason why we pack vessels at close distance when landed...
                switch (Vessel.situation)
                {
                    case Vessel.Situations.LANDED:
                    case Vessel.Situations.SPLASHED:
                        if (!Vessel.packed && FlightGlobals.ActiveVessel.id != VesselId && Vessel.isEVA)
                        {
                            //Only call this when the kerbals are in the ground and VERY close
                            Vessel.SetPosition(curPosition);
                        }
                        Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], lerpPercentage);
                        Vessel.longitude = Lerp(LatLonAlt[1], Target.LatLonAlt[1], lerpPercentage);
                        Vessel.altitude = Lerp(LatLonAlt[2], Target.LatLonAlt[2], lerpPercentage);
                        //DO NOT call Vessel.orbitDriver.updateFromParameters when landed as vessel jitters up/down when unpacked
                        break;
                    case Vessel.Situations.FLYING:
                    case Vessel.Situations.SUB_ORBITAL:
                    case Vessel.Situations.ORBITING:
                    case Vessel.Situations.ESCAPING:
                    case Vessel.Situations.DOCKED:
                        //Vessel.heightFromTerrain = Target.Height; //NO need to set the height from terrain, not even in flying
                        Vessel.orbitDriver.updateFromParameters();
                        //This does not seems to affect when the vessel is landed but I moved it to orbiting to increase performance
                        foreach (var part in Vessel.Parts)
                            part.ResumeVelocity();
                        break;
                }
            }
        }

        /// <summary>
        /// Here we apply the CURRENT vessel position to this update.
        /// </summary>
        private void ProcessRestart()
        {
            RestartRequested = false;
            if (!SettingsSystem.CurrentSettings.InterpolationEnabled)
                return;

            SrfRelRotation = new[] {
                Vessel.srfRelRotation.x,
                Vessel.srfRelRotation.y,
                Vessel.srfRelRotation.z,
                Vessel.srfRelRotation.w
            };

            TransformPosition = new[]
            {
                (double)Vessel.ReferenceTransform.position.x,
                (double)Vessel.ReferenceTransform.position.y,
                (double)Vessel.ReferenceTransform.position.z
            };
            Velocity = new[]
            {
                (double)Vessel.velocityD.x,
                (double)Vessel.velocityD.y,
                (double)Vessel.velocityD.z,
            };
            LatLonAlt = new[]
            {
                Vessel.latitude,
                Vessel.longitude,
                Vessel.altitude,
            };
            Com = new[]
            {
                (double)Vessel.CoM.x,
                (double)Vessel.CoM.y,
                (double)Vessel.CoM.z,
            };
            NormalVector = new[]
            {
                (double)Vessel.terrainNormal.x,
                (double)Vessel.terrainNormal.y,
                (double)Vessel.terrainNormal.z,
            };
            Orbit = new[]
            {
                Vessel.orbit.inclination,
                Vessel.orbit.eccentricity,
                Vessel.orbit.semiMajorAxis,
                Vessel.orbit.LAN,
                Vessel.orbit.argumentOfPeriapsis,
                Vessel.orbit.meanAnomalyAtEpoch,
                Vessel.orbit.epoch,
                Vessel.orbit.referenceBody.flightGlobalsIndex
            };
            Landed = Vessel.Landed;
            Splashed = Vessel.Splashed;
        }

        #region Helper methods

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        private static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        #endregion

        #endregion
    }
}
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

// ReSharper disable All

namespace LunaClient.Systems.VesselPositionAltSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionAltUpdate : VesselPositionMsgData
    {
        #region Fields
        public Vessel Vessel => FlightGlobals.Vessels.Find(v => v.id == VesselId);

        private CelestialBody _body;
        public CelestialBody Body
        {
            get
            {
                if (_body == null)
                {
                    _body = FlightGlobals.Bodies.Find(b => b.bodyName == BodyName);
                }

                return _body;
            }
            set { _body = value; }
        }

        private VesselPositionAltUpdate _target;

        public VesselPositionAltUpdate Target
        {
            get
            {
                if (_target == null)
                    VesselPositionAltSystem.TargetVesselUpdate.TryGetValue(VesselId, out _target);
                return _target;
            }
        }

        #region Vessel position information fields

        public Quaternion Rotation => new Quaternion(TransformRotation[0], TransformRotation[1], TransformRotation[2], TransformRotation[3]);
        public Vector3 TransformPos => new Vector3d(TransformPosition[0], TransformPosition[1], TransformPosition[2]);
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
        public bool InterpolationFinished { get; set; }
        public float InterpolationDuration { get; set; }
        public float LerpPercentage { get; set; } = 0;

        #endregion

        #endregion

        #region Constructors/Creation

        public VesselPositionAltUpdate(VesselPositionMsgData parent)
        {
            Velocity = parent.Velocity;
            VesselId = parent.VesselId;
            Acceleration = parent.Acceleration;
            BodyName = parent.BodyName;
            GameSentTime = parent.GameSentTime;
            Landed = parent.Landed;
            LatLonAlt = parent.LatLonAlt;
            Height = parent.Height;
            NormalVector = parent.NormalVector;
            Com = parent.Com;
            Orbit = parent.Orbit;
            OrbitPosition = parent.OrbitPosition;
            OrbitVelocity = parent.OrbitVelocity;
            PlanetTime = parent.PlanetTime;
            ReceiveTime = parent.ReceiveTime;
            RefTransformPos = parent.RefTransformPos;
            RefTransformRot = parent.RefTransformRot;
            SentTime = parent.SentTime;
            TransformPosition = parent.TransformPosition;
            TransformRotation = parent.TransformRotation;
        }

        public VesselPositionAltUpdate(Vessel vessel)
        {
            try
            {
                VesselId = vessel.id;
                BodyName = vessel.mainBody.bodyName;

                TransformRotation = new[]
                {
                    vessel.ReferenceTransform.rotation.x,
                    vessel.ReferenceTransform.rotation.y,
                    vessel.ReferenceTransform.rotation.z,
                    vessel.ReferenceTransform.rotation.w
                };
                TransformPosition = new[]
                {
                    (double)vessel.ReferenceTransform.position.x,
                    (double)vessel.ReferenceTransform.position.y,
                    (double)vessel.ReferenceTransform.position.z
                };
                Velocity = new[]
                {
                    (double)vessel.rb_velocity.x,
                    (double)vessel.rb_velocity.y,
                    (double)vessel.rb_velocity.z,
                };
                LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude,
                };
                Height = vessel.heightFromTerrain;
                Com = new[]
                {
                    (double)vessel.CoM.x,
                    (double)vessel.CoM.x,
                    (double)vessel.CoM.z,
                };
                NormalVector = new[]
                {
                    (double)vessel.terrainNormal.x,
                    (double)vessel.terrainNormal.y,
                    (double)vessel.terrainNormal.z,
                };
                Orbit = new[]
                {
                    vessel.orbit.inclination,
                    vessel.orbit.eccentricity,
                    vessel.orbit.semiMajorAxis,
                    vessel.orbit.LAN,
                    vessel.orbit.argumentOfPeriapsis,
                    vessel.orbit.meanAnomalyAtEpoch,
                    vessel.orbit.epoch,
                    vessel.orbit.referenceBody.flightGlobalsIndex
                };
            }
            catch (Exception e)
            {
                LunaLog.Log($"[LMP]: Failed to get vessel position update, exception: {e}");
            }
        }

        public new virtual VesselPositionAltUpdate Clone()
        {
            return MemberwiseClone() as VesselPositionAltUpdate;
        }

        #endregion

        #region Main method

        public void ApplyVesselUpdate()
        {
            try
            {
                if (Body == null || Vessel == null || Vessel.precalc == null || Target == null)
                {
                    VesselPositionAltSystem.VesselsToRemove.Enqueue(VesselId);
                    return;
                }

                if (!InterpolationStarted)
                {
                    var interval = (float)TimeSpan.FromTicks(Target.SentTime - SentTime).TotalSeconds;
                    InterpolationDuration = Mathf.Clamp(interval, 0, 0.5f);
                    InterpolationStarted = true;
                }

                if (InterpolationDuration > 0)
                {
                    if (SettingsSystem.CurrentSettings.InterpolationEnabled && LerpPercentage < 1)
                    {
                        ApplyInterpolations(LerpPercentage);
                        LerpPercentage += Time.fixedDeltaTime / InterpolationDuration;
                        return;
                    }

                    if (!SettingsSystem.CurrentSettings.InterpolationEnabled)
                        ApplyInterpolations(1);
                }

                InterpolationFinished = true;
            }
            catch
            {

            }
        }

        private void ApplyInterpolations(float lerpPercentage)
        {
            var tgtOrbit = new Orbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3],
                Target.Orbit[4], Target.Orbit[5], Target.Orbit[6], Body);

            CopyOrbit(tgtOrbit, Vessel.orbitDriver.orbit);
            Vessel.orbitDriver.orbit.Init();

            Vessel.CoM = Vector3.Lerp(CoM, Target.CoM, lerpPercentage);
            Vessel.terrainNormal = Vector3.Lerp(Normal, Target.Normal, lerpPercentage);
            if (!Vessel.loaded)
            {
                //DO NOT lerp the latlonalt as otherwise if you are in 
                //orbit you will see landed vessels in the map view with weird jittering
                Vessel.latitude = Target.LatLonAlt[0];
                Vessel.longitude = Target.LatLonAlt[1];
                Vessel.altitude = Target.LatLonAlt[2];
                Vessel.heightFromTerrain = Target.Height;
            }
            else
            {
                var curRotation = Quaternion.Lerp(Rotation, Target.Rotation, lerpPercentage);
                var curPosition = Vector3.Lerp(TransformPos, Target.TransformPos, lerpPercentage);
                var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage);

                Vessel.SetWorldVelocity(curVelocity);
                Vessel.ReferenceTransform.position = curPosition;
                Vessel.ReferenceTransform.rotation = curRotation;
                Vessel.SetRotation(curRotation, true);

                Vessel.srfRelRotation = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * curRotation;
                Vessel.mainBody.GetLatLonAlt(curPosition, out Vessel.latitude, out Vessel.longitude,
                    out Vessel.altitude);
            }
            
            //if (SettingsSystem.CurrentSettings.Debug2)
            //{
            //    Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos;
            //    Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel;
            //}
            //if (SettingsSystem.CurrentSettings.Debug3)
            //    Vessel.orbitDriver.orbit.UpdateFromUT(Planetarium.GetUniversalTime());
            //if (SettingsSystem.CurrentSettings.Debug4)
            //    Vessel.orbitDriver.UpdateOrbit();
            //if (SettingsSystem.CurrentSettings.Debug5)
            //    Vessel.orbitDriver.UpdateOrbit(false);
            //if (SettingsSystem.CurrentSettings.Debug6)
            //    Vessel.orbitDriver.updateFromParameters();
        }

        /// <summary>
        /// Custom lerp as Unity does not have a lerp for double values
        /// </summary>
        private static double Lerp(double from, double to, float t)
        {
            return from * (1 - t) + to * t;
        }

        private static Orbit Lerp(Orbit from, Orbit to, float t)
        {
            return new Orbit()
            {
                inclination = from.inclination * (1 - t) + to.inclination * t,
                eccentricity = from.eccentricity * (1 - t) + to.eccentricity * t,
                semiMajorAxis = from.semiMajorAxis * (1 - t) + to.semiMajorAxis * t,
                LAN = from.LAN * (1 - t) + to.LAN * t,
                argumentOfPeriapsis = from.argumentOfPeriapsis * (1 - t) + to.argumentOfPeriapsis * t,
                meanAnomalyAtEpoch = from.meanAnomalyAtEpoch * (1 - t) + to.meanAnomalyAtEpoch * t,
                epoch = from.epoch * (1 - t) + to.epoch * t,
            };
        }

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
        }


        #endregion

        public VesselPositionMsgData AsSimpleMessage()
        {
            return new VesselPositionMsgData
            {
                Velocity = Velocity,
                VesselId = VesselId,
                Acceleration = Acceleration,
                BodyName = BodyName,
                GameSentTime = GameSentTime,
                Landed = Landed,
                LatLonAlt = LatLonAlt,
                Height = Height,
                Com = Com,
                NormalVector = NormalVector,
                Orbit = Orbit,
                OrbitPosition = OrbitPosition,
                OrbitVelocity = OrbitVelocity,
                PlanetTime = PlanetTime,
                ReceiveTime = ReceiveTime,
                RefTransformPos = RefTransformPos,
                RefTransformRot = RefTransformRot,
                SentTime = SentTime,
                TransformPosition = TransformPosition,
                TransformRotation = TransformRotation,
            };
        }
    }
}
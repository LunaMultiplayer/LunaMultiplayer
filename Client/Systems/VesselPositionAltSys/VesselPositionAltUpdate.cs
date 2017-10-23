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
                    (double) vessel.rb_velocity.x,
                    (double) vessel.rb_velocity.y,
                    (double) vessel.rb_velocity.z,
                };
                LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude
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
            var lerpedOrbit = Lerp(Orbit, Target.Orbit, Body, LerpPercentage);
            CopyOrbit(lerpedOrbit, Vessel.orbitDriver.orbit);
            Vessel.orbitDriver.pos = Vessel.orbitDriver.orbit.pos.xzy;
            Vessel.orbitDriver.vel = Vessel.orbitDriver.orbit.vel.xzy;

            if (!Vessel.loaded)
            {
                Vessel.latitude = Lerp(LatLonAlt[0], Target.LatLonAlt[0], lerpPercentage);
                Vessel.longitude = Lerp(LatLonAlt[0], Target.LatLonAlt[1], lerpPercentage);
                Vessel.altitude = Lerp(LatLonAlt[0], Target.LatLonAlt[2], lerpPercentage);
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
                Vessel.mainBody.GetLatLonAlt(curPosition, out Vessel.latitude, out Vessel.longitude, out Vessel.altitude);
            }
        }

        private void ApplyOrbitData()
        {
            Vessel.orbit.inclination = Orbit[0];
            Vessel.orbit.eccentricity = Orbit[1];
            Vessel.orbit.semiMajorAxis = Orbit[2];
            Vessel.orbit.LAN = Orbit[3];
            Vessel.orbit.argumentOfPeriapsis = Orbit[4];
            Vessel.orbit.meanAnomalyAtEpoch = Orbit[5];
            Vessel.orbit.epoch = Orbit[6];
            Vessel.orbit.referenceBody = Body;
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

        private static Orbit Lerp(double[] from, double[] to, CelestialBody body, float t)
        {
            var fromObt = new Orbit(from[0], from[1], from[2], from[3], from[4], from[5], from[6], body);
            var toObt = new Orbit(to[0], to[1], to[2], to[3], to[4], to[5], to[6], body);

            return Lerp(fromObt, toObt, t);
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
            destinationOrbit.Init();
            destinationOrbit.UpdateFromUT(Planetarium.GetUniversalTime());
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
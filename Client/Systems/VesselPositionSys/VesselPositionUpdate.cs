using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

// ReSharper disable All

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// This class handle the vessel position updates that we received and applies it to the correct vessel. 
    /// It also handle it's interpolations
    /// </summary>
    public class VesselPositionUpdate
    {
        #region Fields

        public VesselPositionMsgData MsgData { get; set; }

        public Vessel Vessel => FlightGlobals.Vessels.Find(v => v.id == MsgData.VesselId);

        private CelestialBody _body;
        public CelestialBody Body
        {
            get
            {
                if (_body == null)
                {
                    _body = FlightGlobals.Bodies.Find(b => b.bodyName == MsgData.BodyName);
                }

                return _body;
            }
            set { _body = value; }
        }

        private VesselPositionUpdate _target;

        public VesselPositionUpdate Target
        {
            get
            {
                if (_target == null)
                    VesselPositionSystem.TargetVesselUpdate.TryGetValue(MsgData.VesselId, out _target);
                return _target;
            }
        }

        #region Vessel position information fields

        public Quaternion Rotation => new Quaternion(MsgData.TransformRotation[0], MsgData.TransformRotation[1], MsgData.TransformRotation[2], MsgData.TransformRotation[3]);
        public Vector3 TransformPos => new Vector3d(MsgData.TransformPosition[0], MsgData.TransformPosition[1], MsgData.TransformPosition[2]);
        public Vector3 CoM => new Vector3d(MsgData.Com[0], MsgData.Com[1], MsgData.Com[2]);
        public Vector3 Normal => new Vector3d(MsgData.NormalVector[0], MsgData.NormalVector[1], MsgData.NormalVector[2]);
        public Vector3d VelocityVector => new Vector3d(MsgData.Velocity[0], MsgData.Velocity[1], MsgData.Velocity[2]);

        private Vector3d _position = Vector3d.zero;
        public Vector3d Position
        {
            get
            {
                if (_position == Vector3d.zero && Body != null)
                {
                    _position = Body.GetWorldSurfacePosition(MsgData.LatLonAlt[0], MsgData.LatLonAlt[1], MsgData.LatLonAlt[2]);
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

        public VesselPositionUpdate(VesselPositionMsgData msgData)
        {
            MsgData = msgData;
        }

        public VesselPositionUpdate(Vessel vessel)
        {
            try
            {
                MsgData.VesselId = vessel.id;
                MsgData.BodyName = vessel.mainBody.bodyName;

                MsgData.TransformRotation = new[]
                {
                    vessel.ReferenceTransform.rotation.x,
                    vessel.ReferenceTransform.rotation.y,
                    vessel.ReferenceTransform.rotation.z,
                    vessel.ReferenceTransform.rotation.w
                };
                MsgData.TransformPosition = new[]
                {
                    (double)vessel.ReferenceTransform.position.x,
                    (double)vessel.ReferenceTransform.position.y,
                    (double)vessel.ReferenceTransform.position.z
                };
                MsgData.Velocity = new[]
                {
                    (double)vessel.rb_velocity.x,
                    (double)vessel.rb_velocity.y,
                    (double)vessel.rb_velocity.z,
                };
                MsgData.LatLonAlt = new[]
                {
                    vessel.latitude,
                    vessel.longitude,
                    vessel.altitude,
                };
                MsgData.Height = vessel.heightFromTerrain;
                MsgData.Com = new[]
                {
                    (double)vessel.CoM.x,
                    (double)vessel.CoM.x,
                    (double)vessel.CoM.z,
                };
                MsgData.NormalVector = new[]
                {
                    (double)vessel.terrainNormal.x,
                    (double)vessel.terrainNormal.y,
                    (double)vessel.terrainNormal.z,
                };
                MsgData.Orbit = new[]
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

        #endregion

        #region Main method

        public void ApplyVesselUpdate()
        {
            try
            {
                if (Body == null || Vessel == null || Vessel.precalc == null || Target == null)
                {
                    VesselPositionSystem.VesselsToRemove.Enqueue(MsgData.VesselId);
                    return;
                }

                if (!InterpolationStarted)
                {
                    var interval = (float)TimeSpan.FromTicks(Target.MsgData.SentTime - MsgData.SentTime).TotalSeconds;
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
            //TODO: Check if this can be improved as it probably creates a lot of garbage in memory. Perhaps we can reuse the Orbit class?
            var tgtOrbit = new Orbit(Target.MsgData.Orbit[0], Target.MsgData.Orbit[1], Target.MsgData.Orbit[2], Target.MsgData.Orbit[3],
                Target.MsgData.Orbit[4], Target.MsgData.Orbit[5], Target.MsgData.Orbit[6], Body);

            CopyOrbit(tgtOrbit, Vessel.orbitDriver.orbit);
            Vessel.orbitDriver.orbit.Init();

            Vessel.CoM = Vector3.Lerp(CoM, Target.CoM, lerpPercentage);
            Vessel.terrainNormal = Vector3.Lerp(Normal, Target.Normal, lerpPercentage);
            if (!Vessel.loaded)
            {
                //DO NOT lerp the latlonalt as otherwise if you are in 
                //orbit you will see landed vessels in the map view with weird jittering
                Vessel.latitude = Target.MsgData.LatLonAlt[0];
                Vessel.longitude = Target.MsgData.LatLonAlt[1];
                Vessel.altitude = Target.MsgData.LatLonAlt[2];
            }
            else
            {
                var curRotation = Quaternion.Lerp(Rotation, Target.Rotation, lerpPercentage);
                var curPosition = Vector3.Lerp(TransformPos, Target.TransformPos, lerpPercentage);
                var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, lerpPercentage);

                //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped
                Vessel.SetWorldVelocity(curVelocity);
                Vessel.ReferenceTransform.rotation = curRotation;
                Vessel.SetRotation(curRotation, true);

                //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
                Vessel.srfRelRotation = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * curRotation;

                //If you do Vessel.ReferenceTransform.position = curPosition 
                //then in orbit vessels crash when they get unpacked and also vessels go inside terrain randomly
                //that is the reason why we pack vessels at close distance when landed...

                switch (Vessel.situation)
                {
                    case Vessel.Situations.LANDED:
                    case Vessel.Situations.SPLASHED:
                        //Ignore the altitude and let the client calculate it when vessel is landed/splashed
                        //Hopefully this will limit the chances where the vessel crashing into terrain
                        Vessel.mainBody.GetLatLonAlt(curPosition, out Vessel.latitude, out Vessel.longitude, out _);
                        Vessel.ReferenceTransform.position = Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude);
                        break;
                    case Vessel.Situations.FLYING:
                    case Vessel.Situations.SUB_ORBITAL:
                        Vessel.heightFromTerrain = Target.MsgData.Height;
                        Vessel.mainBody.GetLatLonAlt(curPosition, out Vessel.latitude, out Vessel.longitude, out Vessel.altitude);
                        Vessel.ReferenceTransform.position = Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude);
                        break;
                    case Vessel.Situations.ORBITING:
                    case Vessel.Situations.ESCAPING:
                    case Vessel.Situations.DOCKED:
                        //DO NOT call updateFromParameters when landed as vessel jitters up/down
                        Vessel.orbitDriver.updateFromParameters();
                        //This does not seems to affect when the vessel is landed but I moved it to orbiting to increase performance
                        foreach (var part in Vessel.Parts)
                            part.ResumeVelocity();
                        break;
                }
            }
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
    }
}
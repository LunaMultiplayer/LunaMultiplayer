using LunaClient.Systems.SettingsSys;
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
        public float Height { get; set; }
        public float[] TransformRotation { get; set; }
        public long SentTime { get; set; }

        #endregion

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

        #region Main method

        public void ResetFields()
        {
            InterpolationStarted = false;
            InterpolationFinished = false;
            InterpolationDuration = 0;
            LerpPercentage = 0;
            _position = Vector3d.zero;
            _body = null;
            _target = null;
        }

        public void ApplyVesselUpdate()
        {
            try
            {
                if (Body == null || Vessel == null || Vessel.precalc == null || Target == null)
                {
                    VesselPositionSystem.VesselsToRemove.Enqueue(VesselId);
                    return;
                }

                if (!InterpolationStarted)
                {
                    Vessel.orbit.referenceBody = Body;
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
            //TODO: Check if this can be improved as it probably creates garbage in memory. Perhaps we can reuse the Orbit class?
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
                        Vessel.mainBody.GetLatLonAlt(curPosition, out Vessel.latitude, out Vessel.longitude, out Vessel.altitude);
                        //No need to do the line below...
                        //Vessel.ReferenceTransform.position = Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude);
                        break;
                    case Vessel.Situations.FLYING:
                    case Vessel.Situations.SUB_ORBITAL:
                        //Vessel.heightFromTerrain = Target.Height;
                        //Vessel.orbitDriver.updateFromParameters();
                        //break;
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

        #endregion

        #region Helper methods

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
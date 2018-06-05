using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.Warp;
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

        public float ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;
        public float InterpolationDuration => Mathf.Clamp((float)(Target.GameTimeStamp - GameTimeStamp) + ExtraInterpolationTime, 0, float.MaxValue);

        private float _lerpPercentage = 1;
        public float LerpPercentage
        {
            get => SettingsSystem.CurrentSettings.PositionInterpolation ? _lerpPercentage : 1;
            set => _lerpPercentage = Mathf.Clamp01(value);
        }

        public double EquivalentGameTime
        {
            get
            {
                if (SubspaceId >= 0)
                {
                    if (WarpSystem.Singleton.Subspaces.TryGetValue(SubspaceId, out var extraTime))
                        return GameTimeStamp - extraTime;
                }

                return GameTimeStamp;
            }
        }

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

        #endregion

        #region Main method

        public void ForceRestart()
        {
            VesselPositionSystem.TargetVesselUpdateQueue[VesselId].Recycle(Target);
            Target = null;
            LerpPercentage = 1;
        }

        /// <summary>
        /// Call this method to apply a vessel update using interpolation
        /// </summary>
        public void ApplyInterpolatedVesselUpdate()
        {
            if (Vessel == null || Vessel.precalc == null || Vessel.state == Vessel.State.DEAD || Body == null)
            {
                return;
            }

            if (InterpolationFinished && VesselPositionSystem.TargetVesselUpdateQueue[VesselId].TryDequeue(out var targetUpdate))
            {
                ProcessRestart();
                LerpPercentage = 0;

                VesselPositionSystem.TargetVesselUpdateQueue[VesselId].Recycle(Target);

                Target = targetUpdate;

                AdjustExtraInterpolationTimes();

                Target.KspOrbit = new Orbit(Target.Orbit[0], Target.Orbit[1], Target.Orbit[2], Target.Orbit[3],
                    Target.Orbit[4], Target.Orbit[5], Target.Orbit[6], Target.Body);

                UpdateProtoVesselValues();
            }

            if (Target == null) return;

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
                LerpPercentage += Time.deltaTime / InterpolationDuration;
            }
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced
        /// </summary>
        private void AdjustExtraInterpolationTimes()
        {
            if (!WarpSystem.Singleton.SubspaceIdIsMoreAdvancedInTime(Target.SubspaceId))
            {
                var queueCount = VesselPositionSystem.TargetVesselUpdateQueue[VesselId].Count;
                //We are more advanced or in the same time. For this case we want to have between 2 and 4 packets in the queue.
                switch (queueCount)
                {
                    case 0:
                        ExtraInterpolationTime = InterpolationDuration * 0.75f;
                        break;
                    case 1:
                        ExtraInterpolationTime = InterpolationDuration * 0.25f;
                        break;
                    case 2:
                    case 3:
                        ExtraInterpolationTime = 0;
                        break;
                    case 4:
                        ExtraInterpolationTime = InterpolationDuration * 0.25f * -1f;
                        break;
                    default:
                        ExtraInterpolationTime = InterpolationDuration * 0.60f * -1f;
                        break;
                }
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
            var currentSurfaceRelRotation = Quaternion.Slerp(SurfaceRelRotation, Target.SurfaceRelRotation, LerpPercentage);
            var curVelocity = Vector3d.Lerp(VelocityVector, Target.VelocityVector, LerpPercentage);

            //Always apply velocity otherwise vessel is not positioned correctly and sometimes it moves even if it should be stopped.
            Vessel.SetWorldVelocity(curVelocity);
            Vessel.velocityD = curVelocity;

            //If you don't set srfRelRotation and vessel is packed it won't change it's rotation
            Vessel.srfRelRotation = currentSurfaceRelRotation;
            Vessel.SetRotation((Quaternion)Body.rotation * currentSurfaceRelRotation, true);

            Vessel.Landed = LerpPercentage < 0.5 ? Landed : Target.Landed;
            Vessel.Splashed = LerpPercentage < 0.5 ? Splashed : Target.Splashed;

            //Set the position of the vessel based on the orbital parameters
            Vessel.orbitDriver.updateFromParameters();

            if (Vessel.LandedOrSplashed)
            {
                Vessel.latitude = LunaMath.Lerp(LatLonAlt[0], Target.LatLonAlt[0], LerpPercentage);
                Vessel.longitude = LunaMath.Lerp(LatLonAlt[1], Target.LatLonAlt[1], LerpPercentage);
                Vessel.altitude = LunaMath.Lerp(LatLonAlt[2], Target.LatLonAlt[2], LerpPercentage);

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
            Vessel.latitude = LunaMath.Lerp(LatLonAlt[0], Target.LatLonAlt[0], LerpPercentage);
            Vessel.longitude = LunaMath.Lerp(LatLonAlt[1], Target.LatLonAlt[1], LerpPercentage);
            Vessel.altitude = LunaMath.Lerp(LatLonAlt[2], Target.LatLonAlt[2], LerpPercentage);

            Vessel.Landed = LerpPercentage < 0.5 ? Landed : Target.Landed;
            Vessel.Splashed = LerpPercentage < 0.5 ? Splashed : Target.Splashed;

            var currentSurfaceRelRotation = Quaternion.Slerp(SurfaceRelRotation, Target.SurfaceRelRotation, LerpPercentage);
            Vessel.SetRotation((Quaternion)Vessel.mainBody.rotation * currentSurfaceRelRotation, true);
            Vessel.srfRelRotation = currentSurfaceRelRotation;

            ApplyOrbitInterpolation();
            Vessel.orbitDriver.updateFromParameters();

            //We don't do the surface positioning as with vessels because kerbals don't walk at high speeds and with this code it will be enough ;)
            if (Vessel.LandedOrSplashed || Vessel.situation <= Vessel.Situations.FLYING)
                Vessel.SetPosition(Body.GetWorldSurfacePosition(Vessel.latitude, Vessel.longitude, Vessel.altitude));
        }

        private void ApplyOrbitInterpolation()
        {
            var lerpTime = LunaMath.Lerp(Orbit[6], Target.Orbit[6], LerpPercentage);

            var currentPos = KspOrbit.getRelativePositionAtUT(Orbit[6]);
            var targetPos = Target.KspOrbit.getRelativePositionAtUT(Target.Orbit[6]);

            var currentVel = KspOrbit.getOrbitalVelocityAtUT(Orbit[6]) + KspOrbit.referenceBody.GetFrameVelAtUT(Orbit[6]) - Body.GetFrameVelAtUT(Orbit[6]);
            var targetVel = Target.KspOrbit.getOrbitalVelocityAtUT(Target.Orbit[6]) + Target.KspOrbit.referenceBody.GetFrameVelAtUT(Target.Orbit[6]) - Target.Body.GetFrameVelAtUT(Target.Orbit[6]);

            var lerpedPos = Vector3d.Lerp(currentPos, targetPos, LerpPercentage);
            var lerpedVel = Vector3d.Lerp(currentVel, targetVel, LerpPercentage);

            Vessel.orbitDriver.orbit.UpdateFromStateVectors(lerpedPos, lerpedVel, LerpBody, lerpTime);
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

            KspOrbit = new Orbit(Orbit[0], Orbit[1], Orbit[2], Orbit[3], Orbit[4], Orbit[5], Orbit[6], Body);
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

        #endregion

        #endregion
    }
}

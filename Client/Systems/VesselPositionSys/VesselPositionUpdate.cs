using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionSys.ExtensionMethods;
using LunaClient.Systems.Warp;
using LunaClient.VesselStore;
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
        public double[] VelocityVector { get; set; } = new double[3];
        public double[] NormalVector { get; set; } = new double[3];
        public double[] Orbit { get; set; } = new double[8];
        public float[] SrfRelRotation { get; set; } = new float[4];
        public float HeightFromTerrain { get; set; }
        public double GameTimeStamp { get; set; }
        public int SubspaceId { get; set; }
        public bool HackingGravity { get; set; }

        #endregion

        #region Vessel position information fields

        public Vector3d Velocity => new Vector3d(VelocityVector[0], VelocityVector[1], VelocityVector[2]);
        public Quaternion SurfaceRelRotation => new Quaternion(SrfRelRotation[0], SrfRelRotation[1], SrfRelRotation[2], SrfRelRotation[3]);
        public Vector3 Normal => new Vector3d(NormalVector[0], NormalVector[1], NormalVector[2]);

        #endregion

        #region Interpolation fields

        private double MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds * 2
            : double.MaxValue;

        private int MessageCount => VesselPositionSystem.TargetVesselUpdateQueue.TryGetValue(VesselId, out var queue) ? queue.Count : 0;
        public double TimeDifference { get; private set; }
        public double ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;
        public double InterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp + ExtraInterpolationTime, 0, MaxInterpolationDuration);

        private float _lerpPercentage = 1;
        public float LerpPercentage
        {
            get => SettingsSystem.CurrentSettings.PositionInterpolation ? _lerpPercentage : 1;
            set => _lerpPercentage = Mathf.Clamp01(value);
        }
        
        public Vector3 PositionVectorDiff = Vector3.zero;

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
            Array.Copy(msgData.LatLonAlt, LatLonAlt, 3);
            Array.Copy(msgData.VelocityVector, VelocityVector, 3);
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
            Array.Copy(update.LatLonAlt, LatLonAlt, 3);
            Array.Copy(update.VelocityVector, VelocityVector, 3);
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
            if (Body == null) return;

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

                Vessel?.protoVessel?.UpdatePositionValues(Target);
                VesselsProtoStore.UpdateVesselProtoPosition(this);
            }

            if (Target == null) return;
            try
            {
                Vessel.SetVesselPosition(this, Target, LerpPercentage);
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
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced.
        /// The idea is that we replay the message at the correct time that is GameTimeWhenMEssageWasSent+InterpolationOffset
        /// In order to adjust we increase or decrease the interpolation duration so next packet matches the time more perfectly
        /// </summary>
        public void AdjustExtraInterpolationTimes()
        {
            if (!SettingsSystem.CurrentSettings.PositionInterpolation)
            {
                TimeDifference = 0;
                return;
            }

            TimeDifference = Planetarium.GetUniversalTime() - GameTimeStamp;

            if (WarpSystem.Singleton.CurrentlyWarping || SubspaceId == -1)
            {
                //This is the case when the message was received while warping or we are warping.

                /* We are warping:
                 * While WE warp if we receive a message that is from before our time, we want to skip it as fast as possible!
                 * If the packet is in the future then we must interpolate towards it
                 *
                 * Player was warping:
                 * The message was received when HE was warping. We don't know his final subspace time BUT if the message was sent
                 * in a time BEFORE ours, we can skip it as fast as possible.
                 * If the packet is in the future then we must interpolate towards it
                 *
                 * Bear in mind that even if the interpolation against the future packet is long because he is in the future,
                 * when we stop warping this method will be called
                 *
                 * Also, we don't remove messages if we are close to the min recommended value
                 *
                 */

                if (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffsetSeconds && MessageCount > VesselPositionSystem.MinRecommendedMessageCount)
                {
                    LerpPercentage = 1;
                }

                ExtraInterpolationTime = Time.fixedDeltaTime;
            }
            else
            {
                //This is the easiest case, the message comes from the same or a past subspace

                //IN past or same subspaces we want to be SettingsSystem.CurrentSettings.InterpolationOffset seconds BEHIND the player position
                if (WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                {
                    /* The subspace is in the past so REMOVE the difference to normalize it
                     * Example: P1 subspace is +7 seconds. Your subspace is + 30 seconds
                     * Packet TimeDifference will be 23 seconds but in reality it should be 0
                     * So, we remove the time difference between subspaces (30 - 7 = 23)
                     * And now the TimeDifference - 23 = 0
                     */
                    var timeToAdd = Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference -= timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffsetSeconds ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        /// <summary>
        /// This gives the fix factor. It scales up or down depending on the error we have
        /// </summary>
        private double GetInterpolationFixFactor()
        {
            //The minimum fix factor is Time.fixedDeltaTime. Usually 0.02 s

            var errorInSeconds = Math.Abs(Math.Abs(TimeDifference) - SettingsSystem.CurrentSettings.InterpolationOffsetSeconds);
            var errorInFrames = errorInSeconds / Time.fixedDeltaTime;

            //We cannot fix errors that are below the delta time!
            if (errorInFrames < 1)
                return 0;

            if (errorInFrames <= 2)
            {
                //The error is max 2 frames ahead/below
                return Time.fixedDeltaTime;
            }
            if (errorInFrames <= 5)
            {
                //The error is max 5 frames ahead/below
                return Time.fixedDeltaTime * 2;
            }
            if (errorInSeconds <= 2.5)
            {
                //The error is max 2.5 SECONDS ahead/below
                return Time.fixedDeltaTime * errorInFrames / 2;
            }

            //The error is really big...
            return Time.fixedDeltaTime * errorInFrames;
        }

        #endregion

        #region Private

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
                Array.Copy(Target.LatLonAlt, LatLonAlt, 3);
                Array.Copy(Target.VelocityVector, VelocityVector, 3);
                Array.Copy(Target.NormalVector, NormalVector, 3);
                Array.Copy(Target.Orbit, Orbit, 8);

                HeightFromTerrain = Target.HeightFromTerrain;
                HackingGravity = Target.HackingGravity;
            }
            else
            {
                if (Vessel == null) return;

                BodyIndex = Vessel.mainBody.flightGlobalsIndex;
                Landed = Vessel.Landed;
                Splashed = Vessel.Splashed;

                SrfRelRotation[0] = Vessel.srfRelRotation.x;
                SrfRelRotation[1] = Vessel.srfRelRotation.y;
                SrfRelRotation[2] = Vessel.srfRelRotation.z;
                SrfRelRotation[3] = Vessel.srfRelRotation.w;

                LatLonAlt[0] = Vessel.latitude;
                LatLonAlt[1] = Vessel.longitude;
                LatLonAlt[2] = Vessel.altitude;

                var velVector = Quaternion.Inverse(Vessel.mainBody.bodyTransform.rotation) * Vessel.srf_velocity;
                VelocityVector[0] = velVector.x;
                VelocityVector[1] = velVector.y;
                VelocityVector[2] = velVector.z;

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

        #endregion

        #endregion
    }
}

using System;
using LmpClient.Extensions;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using LmpCommon;
using LmpCommon.Message.Data.Vessel;
using UnityEngine;

namespace LmpClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateUpdate
    {
        #region Fields

        public VesselFlightStateUpdate Target { get; set; }

        #region Message Fields

        public FlightCtrlState InterpolatedCtrlState { get; set; } = new FlightCtrlState();
        public FlightCtrlState CtrlState { get; set; } = new FlightCtrlState();
        public double GameTimeStamp { get; set; }
        public int SubspaceId { get; set; }
        public Guid VesselId { get; set; }

        #endregion

        #region Interpolation fields

        private double MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds * 2
            : double.MaxValue;

        private int MessageCount => VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(VesselId, out var queue) ? queue.Count : 0;
        public double TimeDifference { get; private set; }
        public double ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;

        public double InterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp + ExtraInterpolationTime, 0, MaxInterpolationDuration);

        public float LerpPercentage { get; set; } = 1;

        #endregion

        #endregion

        #region Constructor

        public VesselFlightStateUpdate() { }
        
        public VesselFlightStateUpdate(VesselFlightStateMsgData msgData)
        {
            VesselId = msgData.VesselId;
            GameTimeStamp = msgData.GameTime;
            SubspaceId = msgData.SubspaceId;

            CtrlState.CopyFrom(msgData);
        }

        public void CopyFrom(VesselFlightStateUpdate update)
        {
            VesselId = update.VesselId;
            GameTimeStamp = update.GameTimeStamp;
            SubspaceId = update.SubspaceId;

            CtrlState.CopyFrom(update.CtrlState);
        }

        #endregion


        #region Main method

        /// <summary>
        /// Call this method to apply a vessel update using interpolation
        /// </summary>
        public FlightCtrlState GetInterpolatedValue()
        {
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.id == VesselId)
            {
                //Do not apply flight states updates to our OWN controlled vessel
                return FlightGlobals.ActiveVessel.ctrlState;
            }

            if (InterpolationFinished && VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(VesselId, out var queue) && queue.TryDequeue(out var targetUpdate))
            {
                if (Target == null) //This is the case of first iteration
                    GameTimeStamp = targetUpdate.GameTimeStamp - TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval).TotalSeconds;

                ProcessRestart();
                LerpPercentage = 0;

                if (Target != null)
                {
                    Target.CopyFrom(targetUpdate);
                    VesselFlightStateSystem.TargetFlightStateQueue[VesselId].Recycle(targetUpdate);
                }
                else
                {
                    Target = targetUpdate;
                }

                AdjustExtraInterpolationTimes();

                //UpdateProtoVesselValues();
            }

            if (Target == null) return InterpolatedCtrlState;

            InterpolatedCtrlState.Lerp(CtrlState, Target.CtrlState, LerpPercentage);
            LerpPercentage += (float)(Time.fixedDeltaTime / InterpolationDuration);

            return InterpolatedCtrlState;
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced.
        /// The idea is that we replay the message at the correct time that is GameTimeWhenMEssageWasSent+InterpolationOffset
        /// In order to adjust we increase or decrease the interpolation duration so next packet matches the time more perfectly
        /// </summary>
        public void AdjustExtraInterpolationTimes()
        {
            TimeDifference = TimeSyncSystem.UniversalTime - GameTimeStamp;

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

                if (TimeDifference > VesselCommon.PositionAndFlightStateMessageOffsetSec && MessageCount > VesselFlightStateSystem.MinRecommendedMessageCount)
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

                ExtraInterpolationTime = (TimeDifference > VesselCommon.PositionAndFlightStateMessageOffsetSec ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        /// <summary>
        /// This gives the fix factor. It scales up or down depending on the error we have
        /// </summary>
        private double GetInterpolationFixFactor()
        {
            //The minimum fix factor is Time.fixedDeltaTime. Usually 0.02 seconds

            var errorInSeconds = Math.Abs(Math.Abs(TimeDifference) - VesselCommon.PositionAndFlightStateMessageOffsetSec);
            var errorInFrames = errorInSeconds / Time.fixedDeltaTime;

            //We cannot fix errors that are below the fixed delta time!
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

        /// <summary>
        /// Here we apply the CURRENT vessel flight state to this update.
        /// </summary>
        private void ProcessRestart()
        {
            if (Target != null)
            {
                GameTimeStamp = Target.GameTimeStamp;
                SubspaceId = Target.SubspaceId;

                CtrlState.CopyFrom(Target.CtrlState);
            }
            else
            {
                var vessel = FlightGlobals.fetch.LmpFindVessel(VesselId);
                if (vessel == null) return;

                CtrlState.CopyFrom(vessel.ctrlState);
            }
        }
    }
}

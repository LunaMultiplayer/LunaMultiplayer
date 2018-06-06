using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateUpdate
    {
        private float MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            1 : float.MaxValue;

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

        public float TimeDifference { get; private set; }
        public float ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;
        public float InterpolationDuration => Mathf.Clamp((float)(Target.GameTimeStamp - GameTimeStamp) + ExtraInterpolationTime, 0, MaxInterpolationDuration);

        private float _lerpPercentage = 1;
        public float LerpPercentage
        {
            get => _lerpPercentage;
            set => _lerpPercentage = Mathf.Clamp01(value);
        }

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

        public void ForceRestart()
        {
            VesselFlightStateSystem.TargetFlightStateQueue[VesselId].Recycle(Target);
            Target = null;
            LerpPercentage = 1;
        }

        /// <summary>
        /// Call this method to apply a vessel update using interpolation
        /// </summary>
        public FlightCtrlState GetInterpolatedValue()
        {
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == VesselId)
            {
                //Do not apply flight states updates to our OWN controlled vessel
                return FlightGlobals.ActiveVessel.ctrlState;
            }

            if (InterpolationFinished && VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(VesselId, out var queue) && queue.TryDequeue(out var targetUpdate))
            {
                if (Target == null) //This is the case of first iteration
                    GameTimeStamp = targetUpdate.GameTimeStamp - 1;

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

            InterpolatedCtrlState.Lerp(CtrlState, Target.CtrlState, _lerpPercentage);
            LerpPercentage += Time.fixedDeltaTime / InterpolationDuration;

            return InterpolatedCtrlState;
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced
        /// </summary>
        private void AdjustExtraInterpolationTimes()
        {
            var timeBack = 0.35;

            TimeDifference = (float)(TimeSyncerSystem.UniversalTime - GameTimeStamp);
            if (SubspaceId == -1)
            {
                ExtraInterpolationTime = (TimeDifference > timeBack ? -1 : 0) * GetInterpolationFixFactor();
            }
            else if (WarpSystem.Singleton.SubspaceIsEqualOrInThePast(SubspaceId))
            {
                if (WarpSystem.Singleton.CurrentSubspace != SubspaceId)
                {
                    var timeToAdd = (float)Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference += timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > timeBack ? -1 : 1) * GetInterpolationFixFactor();
            }
            else
            {
                //Future subspace
                ExtraInterpolationTime = (TimeDifference > 0 ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        private float GetInterpolationFixFactor()
        {
            var error = Math.Abs(TimeDifference);
            if (error <= 1.5f)
            {
                return InterpolationDuration * 0.25f;
            }

            if (error <= 5)
            {
                return InterpolationDuration * error / 10;
            }

            if (error <= 10)
            {
                return InterpolationDuration * error / 5;
            }

            if (error <= 15)
            {
                return InterpolationDuration * error / 2;
            }

            return InterpolationDuration * error;
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
                var vessel = FlightGlobals.FindVessel(VesselId);
                if (vessel == null) return;

                CtrlState.CopyFrom(vessel.ctrlState);
            }
        }
    }
}

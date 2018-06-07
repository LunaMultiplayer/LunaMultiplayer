using LunaClient.Systems.SettingsSys;
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
            1 * 10 : float.MaxValue;

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
        public float RawInterpolationDuration => Mathf.Clamp((float)(Target.GameTimeStamp - GameTimeStamp), 0, MaxInterpolationDuration);
        public float InterpolationDuration => Mathf.Clamp((float)(Target.GameTimeStamp - GameTimeStamp) + ExtraInterpolationTime, 0, MaxInterpolationDuration);

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

            InterpolatedCtrlState.LerpUnclamped(CtrlState, Target.CtrlState, LerpPercentage);
            LerpPercentage += Time.fixedDeltaTime / InterpolationDuration;

            return InterpolatedCtrlState;
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced
        /// </summary>
        private void AdjustExtraInterpolationTimes()
        {
            TimeDifference = (float)(TimeSyncerSystem.UniversalTime - GameTimeStamp);
            if (SubspaceId == -1)
            {
                //While warping we only fix the interpolation if we are LAGGING behind the updates
                //We never fix it if the packet that we received is very advanced in time
                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffset ? -1 : 0) * GetInterpolationFixFactor();
            }
            else if (WarpSystem.Singleton.SubspaceIsEqualOrInThePast(SubspaceId))
            {
                //IN past or same subspaces we want to be timeBack seconds BEHIND the player position
                if (WarpSystem.Singleton.CurrentSubspace != SubspaceId)
                {
                    var timeToAdd = (float)Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference += timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffset ? -1 : 1) * GetInterpolationFixFactor();
            }
            else
            {
                //In future subspaces we want to be in the exact time as when the packet was sent
                ExtraInterpolationTime = (TimeDifference > 0 ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        private float GetInterpolationFixFactor()
        {
            var error = Math.Abs(TimeDifference);
            if (error <= 5)
            {
                return RawInterpolationDuration * 0.25f;
            }

            if (error <= 10)
            {
                return RawInterpolationDuration * 0.60f;
            }

            if (error <= 15)
            {
                return RawInterpolationDuration;
            }

            return RawInterpolationDuration * 2;
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

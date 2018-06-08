using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using LunaCommon;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    public class VesselFlightStateUpdate
    {
        private double MaxInterpolationDuration => WarpSystem.Singleton.SubspaceIsEqualOrInThePast(Target.SubspaceId) ?
            TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselPositionUpdatesMsInterval).TotalSeconds * 10
            : double.MaxValue;

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

        public double TimeDifference { get; private set; }
        public double ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;

        public double RawInterpolationDuration => LunaMath.Clamp(Target.GameTimeStamp - GameTimeStamp, 0, MaxInterpolationDuration);
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
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == VesselId)
            {
                //Do not apply flight states updates to our OWN controlled vessel
                return FlightGlobals.ActiveVessel.ctrlState;
            }

            if (InterpolationFinished && VesselFlightStateSystem.TargetFlightStateQueue.TryGetValue(VesselId, out var queue) && queue.TryDequeue(out var targetUpdate))
            {
                if (Target == null) //This is the case of first iteration
                    GameTimeStamp = targetUpdate.GameTimeStamp - TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselPositionUpdatesMsInterval).TotalSeconds;

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
            if (LerpPercentage > 1)
            {
                //We only send flight states of the ACTIVE vessel so perhgaps some player switched a vessel and we are not receiveing any flight state
                //To solve this just remove the vessel from the system
                VesselFlightStateSystem.Singleton.RemoveVesselFromSystem(VesselId);
            }

            InterpolatedCtrlState.Lerp(CtrlState, Target.CtrlState, LerpPercentage);
            LerpPercentage += (float)(Time.fixedDeltaTime / InterpolationDuration);

            return InterpolatedCtrlState;
        }

        /// <summary>
        /// This method adjust the extra interpolation duration in case we are lagging or too advanced
        /// </summary>
        private void AdjustExtraInterpolationTimes()
        {
            TimeDifference = TimeSyncerSystem.UniversalTime - GameTimeStamp;
            if (SubspaceId == -1)
            {
                //While warping we only fix the interpolation if we are LAGGING behind the updates
                //We never fix it if the packet that we received is very advanced in time
                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffset ? -1 : 0) * GetInterpolationFixFactor();
            }
            else 
            {
                //IN past or same subspaces we want to be SettingsSystem.CurrentSettings.InterpolationOffset seconds BEHIND the player position
                if (WarpSystem.Singleton.SubspaceIsInThePast(SubspaceId))
                {
                    //The subspace is in the past so add the difference seconds to normalize it
                    var timeToAdd = Math.Abs(WarpSystem.Singleton.GetTimeDifferenceWithGivenSubspace(SubspaceId));
                    TimeDifference += timeToAdd;
                }

                ExtraInterpolationTime = (TimeDifference > SettingsSystem.CurrentSettings.InterpolationOffset ? -1 : 1) * GetInterpolationFixFactor();
            }
        }

        /// <summary>
        /// This gives the fix factor. It scales up or down depending on the error we have
        /// </summary>
        private double GetInterpolationFixFactor()
        {
            var error = TimeSpan.FromSeconds(Math.Abs(TimeDifference)).TotalMilliseconds;

            //Do not use less than 0.25 as otherwise it won't fix it.
            if (error <= 500)
            {
                return RawInterpolationDuration * 0.25;
            }

            if (error <= 1000)
            {
                return RawInterpolationDuration * 0.60;
            }

            if (error <= 5000)
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

using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
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

        public float ExtraInterpolationTime { get; private set; }
        public bool InterpolationFinished => Target == null || LerpPercentage >= 1;
        public float InterpolationDuration => Mathf.Clamp((float)(Target.GameTimeStamp - GameTimeStamp) + ExtraInterpolationTime, 0, float.MaxValue);

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

        #endregion


        #region Main method

        public void ForceRestart()
        {
            ProcessRestart();
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
                ProcessRestart();
                LerpPercentage = 0;

                VesselFlightStateSystem.TargetFlightStateQueue[VesselId].Recycle(Target);

                Target = targetUpdate;

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
            if (!WarpSystem.Singleton.SubspaceIdIsMoreAdvancedInTime(Target.SubspaceId))
            {
                var queueCount = VesselFlightStateSystem.TargetFlightStateQueue[VesselId].Count;
                //We are more advanced or in the subspace. For this case we want to have between 2 and 4 packets in the queue.
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

using LmpClient.Base;
using LmpClient.Events;
using LmpClient.Systems.SettingsSys;
using LmpClient.Systems.TimeSync;
using LmpClient.Systems.Warp;
using LmpClient.VesselUtilities;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine.Profiling;

namespace LmpClient.Systems.VesselFlightStateSys
{
    /// <summary>
    /// System that controls the flight state (user control inputs) for all the vessels that are in flight, loaded+unpacked and controlled
    /// Basically here we put the other player input (pitch,roll,yaw,...) into their vessels and send our own inputs
    /// </summary>
    [SuppressMessage("ReSharper", "DelegateSubtraction")]
    public class VesselFlightStateSystem : MessageSystem<VesselFlightStateSystem, VesselFlightStateMessageSender, VesselFlightStateMessageHandler>
    {
        #region Fields & properties

        private static DateTime LastVesselFlightStateSentTime { get; set; } = LunaComputerTime.UtcNow;

        private static bool TimeToSendFlightStateUpdate => VesselCommon.PlayerVesselsNearby() ?
            (LunaComputerTime.UtcNow - LastVesselFlightStateSentTime).TotalMilliseconds > SettingsSystem.ServerSettings.VesselUpdatesMsInterval :
            (LunaComputerTime.UtcNow - LastVesselFlightStateSentTime).TotalMilliseconds > SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval;

        public bool FlightStateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && HighLogic.LoadedScene == GameScenes.FLIGHT &&
                                              FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                              FlightGlobals.ActiveVessel.state != Vessel.State.DEAD &&
                                              FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public FlightStateEvents FlightStateEvents { get; } = new FlightStateEvents();

        /// <summary>
        /// This dictionary links a vessel with a callback that will apply the latest flight state we received
        /// </summary>
        public ConcurrentDictionary<Guid, FlightInputCallback> FlyByWireDictionary { get; } =
            new ConcurrentDictionary<Guid, FlightInputCallback>();

        /// <summary>
        /// This dictionary contains the current flight state of a vessel
        /// </summary>
        public static ConcurrentDictionary<Guid, VesselFlightStateUpdate> CurrentFlightState { get; } =
            new ConcurrentDictionary<Guid, VesselFlightStateUpdate>();

        /// <summary>
        /// This dictionary contains a queue with the latest flight states we received
        /// </summary>
        public static ConcurrentDictionary<Guid, FlightStateQueue> TargetFlightStateQueue { get; } =
            new ConcurrentDictionary<Guid, FlightStateQueue>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselFlightStateSystem);

        /// <inheritdoc />
        /// <summary>
        /// This system is multithreaded as we can receive the messages in one thread and store them in a concurrent dictionary
        /// </summary>
        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            GameEvents.onVesselGoOnRails.Add(FlightStateEvents.OnVesselPack);
            GameEvents.onVesselGoOffRails.Add(FlightStateEvents.OnVesselUnpack);

            SpectateEvent.onStartSpectating.Add(FlightStateEvents.OnStartSpectating);
            SpectateEvent.onFinishedSpectating.Add(FlightStateEvents.OnFinishedSpectating);

            //Send the flight state updates after all the calculations are done.
            TimingManager.LateUpdateAdd(TimingManager.TimingStage.BetterLateThanNever, SendFlightState);

            WarpEvent.onTimeWarpStopped.Add(FlightStateEvents.WarpStopped);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            GameEvents.onVesselGoOnRails.Remove(FlightStateEvents.OnVesselPack);
            GameEvents.onVesselGoOffRails.Remove(FlightStateEvents.OnVesselUnpack);

            SpectateEvent.onStartSpectating.Remove(FlightStateEvents.OnStartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(FlightStateEvents.OnFinishedSpectating);

            TimingManager.LateUpdateRemove(TimingManager.TimingStage.BetterLateThanNever, SendFlightState);

            WarpEvent.onTimeWarpStopped.Remove(FlightStateEvents.WarpStopped);

            ClearSystem();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Clear the delegates and the dictionaries
        /// </summary>
        public void ClearSystem()
        {
            foreach (var keyVal in FlyByWireDictionary)
            {
                var vessel = FlightGlobals.FindVessel(keyVal.Key);
                if (vessel != null)
                {
                    try
                    {
                        vessel.OnFlyByWire -= FlyByWireDictionary[keyVal.Key];
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            FlyByWireDictionary.Clear();
            CurrentFlightState.Clear();
            TargetFlightStateQueue.Clear();
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Sends our current flight state. We only send flight states of the ACTIVE vessel if we are not spectating and also if that vessel is not an EVA
        /// </summary>
        private void SendFlightState()
        {
            Profiler.BeginSample(nameof(SendFlightState));

            if (FlightStateSystemReady && TimeToSendFlightStateUpdate && !VesselCommon.IsSpectating && !FlightGlobals.ActiveVessel.isEVA)
            {
                MessageSender.SendCurrentFlightState();
                LastVesselFlightStateSentTime = LunaComputerTime.UtcNow;
            }

            Profiler.EndSample();
        }

        #endregion

        #region Public methods

        public void AddVesselToSystem(Vessel vessel)
        {
            if (vessel == null || vessel.isEVA) return;

            //We must never have our own active and controlled vessel in the dictionary
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel == vessel)
                return;

            FlyByWireDictionary.TryAdd(vessel.id, st => LunaOnVesselFlyByWire(vessel.id, st));

            if (vessel.OnFlyByWire.GetInvocationList().All(d => d.Method.Name != nameof(LunaOnVesselFlyByWire)))
                vessel.OnFlyByWire += FlyByWireDictionary[vessel.id];
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            FlyByWireDictionary.TryRemove(vesselId, out _);
            CurrentFlightState.TryRemove(vesselId, out _);
            TargetFlightStateQueue.TryRemove(vesselId, out _);

            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel != null)
                TryRemoveCallback(vessel);
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        public void RemoveVessel(Vessel vesselToRemove)
        {
            if (vesselToRemove == null) return;

            TryRemoveCallback(vesselToRemove);

            FlyByWireDictionary.TryRemove(vesselToRemove.id, out _);
            CurrentFlightState.TryRemove(vesselToRemove.id, out _);
            TargetFlightStateQueue.TryRemove(vesselToRemove.id, out _);
        }

        /// <summary>
        /// Force adjustment of interpolation times
        /// </summary>
        public void AdjustExtraInterpolationTimes()
        {
            foreach (var keyVal in CurrentFlightState)
            {
                keyVal.Value.AdjustExtraInterpolationTimes();
            }

            //Now cleanup the target dictionary of old positions
            foreach (var keyVal in TargetFlightStateQueue)
            {
                while (keyVal.Value.TryPeek(out var targetFlightState) && FlightStateUpdateIsTooOld(targetFlightState))
                    keyVal.Value.TryDequeue(out _);
            }
        }

        #endregion

        #region Private methods

        private static bool FlightStateUpdateIsTooOld(VesselFlightStateUpdate update)
        {
            var maxInterpolationTime = WarpSystem.Singleton.SubspaceIsEqualOrInThePast(update.SubspaceId) ?
                TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesMsInterval).TotalSeconds * 2
                : double.MaxValue;

            return update.GameTimeStamp < TimeSyncSystem.UniversalTime - maxInterpolationTime;
        }

        /// <summary>
        /// Here we copy the flight state we received and apply to the specific vessel.
        /// This method is called by ksp as it's a delegate. It's called on every FixedUpdate
        /// </summary>
        private void LunaOnVesselFlyByWire(Guid id, FlightCtrlState st)
        {
            if (!Enabled || !FlightStateSystemReady) return;

            if (CurrentFlightState.TryGetValue(id, out var value))
            {
                if (VesselCommon.IsSpectating)
                {
                    st.CopyFrom(value.GetInterpolatedValue());
                }
                else
                {
                    //If we are close to a vessel and we both are in space don't copy the
                    //input controls as then the vessel jitters, specially if the other player has SAS on
                    if (FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.situation > Vessel.Situations.FLYING)
                    {
                        var interpolatedState = value.GetInterpolatedValue();
                        st.mainThrottle = interpolatedState.mainThrottle;
                        st.gearDown = interpolatedState.gearDown;
                        st.gearUp = interpolatedState.gearUp;
                        st.headlight = interpolatedState.headlight;
                        st.killRot = interpolatedState.killRot;
                    }
                    else
                    {
                        st.CopyFrom(value.GetInterpolatedValue());
                    }
                }
            }
        }

        private void TryRemoveCallback(Vessel vesselToRemove)
        {
            if (FlyByWireDictionary.ContainsKey(vesselToRemove.id) && vesselToRemove.OnFlyByWire.GetInvocationList().All(d => d.Method.Name != nameof(LunaOnVesselFlyByWire)))
                vesselToRemove.OnFlyByWire -= FlyByWireDictionary[vesselToRemove.id];
        }

        #endregion
    }
}

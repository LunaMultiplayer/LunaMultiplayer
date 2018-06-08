using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    /// <summary>
    /// System that controls the flight state (user control inputs) for all the vessels that are in flight, loaded+unpacked and controlled
    /// Basically here we put the other player input (pitch,roll,yaw,...) into their vessels and send our own inputs
    /// </summary>
    [SuppressMessage("ReSharper", "DelegateSubtraction")]
    public class VesselFlightStateSystem : MessageSystem<VesselFlightStateSystem, VesselFlightStateMessageSender, VesselFlightStateMessageHandler>
    {
        #region Fields & properties
        
        private static float LastVesselFlightStateSentTime { get; set; }

        private static bool TimeToSendVesselUpdate => VesselCommon.PlayerVesselsNearby() ?
            TimeSpan.FromSeconds(Time.time - LastVesselFlightStateSentTime).TotalMilliseconds > SettingsSystem.ServerSettings.VesselUpdatesMsInterval :
            TimeSpan.FromSeconds(Time.time - LastVesselFlightStateSentTime).TotalMilliseconds > SettingsSystem.ServerSettings.SecondaryVesselUpdatesMsInterval;

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

            LockEvent.onLockAcquire.Add(FlightStateEvents.OnLockAcquire);
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();

            GameEvents.onVesselGoOnRails.Remove(FlightStateEvents.OnVesselPack);
            GameEvents.onVesselGoOffRails.Remove(FlightStateEvents.OnVesselUnpack);

            SpectateEvent.onStartSpectating.Remove(FlightStateEvents.OnStartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(FlightStateEvents.OnFinishedSpectating);

            TimingManager.LateUpdateRemove(TimingManager.TimingStage.BetterLateThanNever, SendFlightState);

            LockEvent.onLockAcquire.Remove(FlightStateEvents.OnLockAcquire);

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
        /// Sends our current flight state
        /// </summary>
        private void SendFlightState()
        {
            if (FlightStateSystemReady && TimeToSendVesselUpdate && !VesselCommon.IsSpectating && !FlightGlobals.ActiveVessel.isEVA)
            {
                MessageSender.SendCurrentFlightState();
                LastVesselFlightStateSentTime = Time.time;
            }
        }

        #endregion

        #region Private methods

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
                    if (FlightGlobals.ActiveVessel?.situation > Vessel.Situations.FLYING)
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

        #region Public methods
        
        public void AddVesselToSystem(Vessel vessel)
        {
            if (vessel == null || vessel.isEVA) return;

            //We must never have our own active and controlled vessel in the dictionary
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == vessel.id)
                return;

            FlyByWireDictionary.TryAdd(vessel.id, st => LunaOnVesselFlyByWire(vessel.id, st));

            if (vessel.OnFlyByWire.GetInvocationList().All(d => d.Method.Name != nameof(LunaOnVesselFlyByWire)))
                vessel.OnFlyByWire += FlyByWireDictionary[vessel.id];
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        public void RemoveVesselFromSystem(Guid vesselId)
        {
            var vessel = FlightGlobals.FindVessel(vesselId);
            if (vessel != null)
                TryRemoveCallback(vessel);

            FlyByWireDictionary.TryRemove(vesselId, out _);
            CurrentFlightState.TryRemove(vesselId, out _);
            TargetFlightStateQueue.TryRemove(vesselId, out _);
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        public void RemoveVesselFromSystem(Vessel vesselToRemove)
        {
            if (vesselToRemove == null) return;

            TryRemoveCallback(vesselToRemove);

            FlyByWireDictionary.TryRemove(vesselToRemove.id, out _);
            CurrentFlightState.TryRemove(vesselToRemove.id, out _);
            TargetFlightStateQueue.TryRemove(vesselToRemove.id, out _);
        }

        public void UpdateFlightStateInProtoVessel(ProtoVessel protoVessel, float pitch, float yaw, float roll, float pitchTrm, float yawTrm, float rollTrm, float throttle)
        {
            if (protoVessel == null) return;

            protoVessel.ctrlState.SetValue("pitch", pitch);
            protoVessel.ctrlState.SetValue("yaw", yaw);
            protoVessel.ctrlState.SetValue("roll", roll);
            protoVessel.ctrlState.SetValue("trimPitch", pitchTrm);
            protoVessel.ctrlState.SetValue("trimYaw", yawTrm);
            protoVessel.ctrlState.SetValue("trimRoll", rollTrm);
            protoVessel.ctrlState.SetValue("mainThrottle", throttle);
        }

        public void UpdateFlightStateInProtoVessel(ProtoVessel protoVessel, VesselFlightStateMsgData msgData)
        {
            UpdateFlightStateInProtoVessel(protoVessel, msgData.Pitch, msgData.Yaw, msgData.Roll, msgData.PitchTrim,
                msgData.YawTrim, msgData.RollTrim, msgData.MainThrottle);
        }

        #endregion
    }
}

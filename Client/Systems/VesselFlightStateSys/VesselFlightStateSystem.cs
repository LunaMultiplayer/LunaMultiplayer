using LunaClient.Base;
using LunaClient.Events;
using LunaClient.VesselUtilities;
using LunaCommon.Message.Data.Vessel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniLinq;
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

        /// <summary>
        /// This dictionary links a vessel with a callback that will apply the latest flight state we received
        /// </summary>
        public Dictionary<Guid, FlightInputCallback> FlyByWireDictionary { get; } =
            new Dictionary<Guid, FlightInputCallback>();

        /// <summary>
        /// This dictioanry contains the latest flight state of a vessel that we received
        /// </summary>
        public ConcurrentDictionary<Guid, VesselFlightStateUpdate> FlightStatesDictionary { get; } =
            new ConcurrentDictionary<Guid, VesselFlightStateUpdate>();

        public bool FlightStateSystemReady
            => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 3f &&
               FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
               FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
               FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public FlightStateEvents FlightStateEvents { get; } = new FlightStateEvents();

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
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SendFlightState));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, ReasignCallbacks));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselGoOnRails.Remove(FlightStateEvents.OnVesselPack);
            GameEvents.onVesselGoOffRails.Remove(FlightStateEvents.OnVesselUnpack);
            SpectateEvent.onStartSpectating.Remove(FlightStateEvents.OnStartSpectating);
            SpectateEvent.onFinishedSpectating.Remove(FlightStateEvents.OnFinishedSpectating);
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
            FlightStatesDictionary.Clear();
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Sends our current flight state
        /// </summary>
        private void SendFlightState()
        {
            if (Enabled && FlightStateSystemReady && !FlightGlobals.ActiveVessel.isEVA)
            {
                MessageSender.SendCurrentFlightState();

                ChangeRoutineExecutionInterval(RoutineExecution.Update, nameof(SendFlightState), VesselCommon.IsSomeoneSpectatingUs ? 30 : 1000);
            }
        }

        public void AddVesselToSystem(Vessel vessel)
        {
            if (vessel == null || vessel.isEVA) return;

            if (!FlyByWireDictionary.ContainsKey(vessel.id))
            {
                //We must never have our own active and controlled vessel in the dictionary
                if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == vessel.id)
                    return;

                FlightStatesDictionary.TryAdd(vessel.id, new VesselFlightStateUpdate());
                FlyByWireDictionary.Add(vessel.id, st => LunaOnVesselFlyByWire(vessel.id, st));

                vessel.OnFlyByWire += FlyByWireDictionary[vessel.id];
            }
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        public void RemoveVesselFromSystem(Vessel vesselToRemove)
        {
            if (vesselToRemove == null) return;

            try
            {
                vesselToRemove.OnFlyByWire -= FlyByWireDictionary[vesselToRemove.id];
            }
            catch (Exception)
            {
                // ignored
            }

            if(FlyByWireDictionary.ContainsKey(vesselToRemove.id))
                FlyByWireDictionary.Remove(vesselToRemove.id);
            FlightStatesDictionary.TryRemove(vesselToRemove.id, out _);
        }
        
        /// <summary>
        /// When vessels are reloaded we must assign the callback back to them
        /// </summary>
        private void ReasignCallbacks()
        {
            if (Enabled && FlightStateSystemReady)
            {
                var vesselsToReasign = FlyByWireDictionary.Keys
                    .Select(FlightGlobals.FindVessel)
                    .Where(v => v != null && !v.OnFlyByWire.GetInvocationList().Any(d => d.Method.Name == nameof(LunaOnVesselFlyByWire)));

                foreach (var vessel in vesselsToReasign)
                {
                    vessel.OnFlyByWire += FlyByWireDictionary[vessel.id];
                }
            }
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

        internal void UpdateFlightStateInProtoVessel(ProtoVessel protoVessel, VesselFlightStateMsgData msgData)
        {
            UpdateFlightStateInProtoVessel(protoVessel, msgData.Pitch, msgData.Yaw, msgData.Roll, msgData.PitchTrim,
                msgData.YawTrim, msgData.RollTrim, msgData.MainThrottle);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Here we copy the flight state we received and apply to the specific vessel.
        /// This method is called by ksp as it's a delegate. It's called on every FixedUpdate
        /// </summary>
        public void LunaOnVesselFlyByWire(Guid id, FlightCtrlState st)
        {
            if (!Enabled || !FlightStateSystemReady) return;
            
            if (FlightStatesDictionary.TryGetValue(id, out var value))
            {
                if (VesselCommon.IsSpectating)
                {
                    st.CopyFrom(value.GetInterpolatedValue(st));
                }
                else
                {
                    //If we are close to a vessel and we both are in space don't copy the
                    //input controls as then the vessel jitters, specially if the other player has SAS on
                    if (FlightGlobals.ActiveVessel?.situation > Vessel.Situations.FLYING)
                    {
                        var interpolatedState = value.GetInterpolatedValue(st);
                        st.mainThrottle = interpolatedState.mainThrottle;
                        st.gearDown = interpolatedState.gearDown;
                        st.gearUp = interpolatedState.gearUp;
                        st.headlight = interpolatedState.headlight;
                        st.killRot = interpolatedState.killRot;
                    }
                }
            }
        }

        #endregion
    }
}

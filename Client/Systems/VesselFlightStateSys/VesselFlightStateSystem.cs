using LunaClient.Base;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    /// <summary>
    /// System that controls the flight state (user control inputs) for all the vessesl that are in flight and controlled
    /// </summary>
    [SuppressMessage("ReSharper", "DelegateSubtraction")]
    public class VesselFlightStateSystem : MessageSystem<VesselFlightStateSystem, VesselFlightStateMessageSender, VesselFlightStateMessageHandler>
    {
        #region Fields & properties

        /// <summary>
        /// This dictionary links a vessel with a callback that will apply the latest flight state we received
        /// </summary>
        private static Dictionary<Guid, FlightInputCallback> FlyByWireDictionary { get; } =
            new Dictionary<Guid, FlightInputCallback>();

        /// <summary>
        /// This dictioanry contains the latest flight state of a vessel that we received
        /// </summary>
        public ConcurrentDictionary<Guid, FlightCtrlState> FlightStatesDictionary { get; } =
            new ConcurrentDictionary<Guid, FlightCtrlState>();

        public bool FlightStateSystemReady
            => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
               FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
               FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
               FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        #endregion

        #region Base overrides

        /// <inheritdoc />
        /// <summary>
        /// This system is multithreaded as we can receive the messages in one thread and store them in a concurrent dictionary
        /// </summary>
        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SendFlightState));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, RemoveUnloadedAndPackedVessels));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, AddLoadedUnpackedVesselsToDictionary));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, ReasignCallbacks));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
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
            if (Enabled && FlightStateSystemReady)
            {
                MessageSender.SendCurrentFlightState();
            }

            if (Enabled)
            {
                ChangeRoutineExecutionInterval("SendFlightState", VesselCommon.IsSomeoneSpectatingUs ? 30 : 500);
            }
        }

        /// <summary>
        /// Removes the unloaded/packed vessels from the system so we don't apply flightstates to them
        /// </summary>
        private void RemoveUnloadedAndPackedVessels()
        {
            if (Enabled && FlightStateSystemReady)
            {
                var vesselsToRemove = FlightGlobals.Vessels
                    .Where(v => (!v.loaded || v.packed) && FlyByWireDictionary.Keys.Contains(v.id));

                foreach (var vesselToRemove in vesselsToRemove)
                {
                    RemoveVesselFromSystem(vesselToRemove);
                }

                //We must never have our own active and controlled vessel in the dictionary
                if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel != null)
                {
                    RemoveVesselFromSystem(FlightGlobals.ActiveVessel);
                }
            }
        }

        /// <summary>
        /// Removes the vessel from the dictionaries
        /// </summary>
        private void RemoveVesselFromSystem(Vessel vesselToRemove)
        {
            try
            {
                vesselToRemove.OnFlyByWire -= FlyByWireDictionary[vesselToRemove.id];
            }
            catch (Exception)
            {
                // ignored
            }

            FlyByWireDictionary.Remove(vesselToRemove.id);
            FlightStatesDictionary.TryRemove(vesselToRemove.id, out _);
        }

        /// <summary>
        /// Adds the loaded and unpacked vessels to the dictionary
        /// </summary>
        private void AddLoadedUnpackedVesselsToDictionary()
        {
            if (Enabled && FlightStateSystemReady)
            {
                var vesselsToAdd = FlightGlobals.VesselsLoaded
                    .Where(v => !v.packed && !FlyByWireDictionary.Keys.Contains(v.id));
                
                foreach (var vesselToAdd in vesselsToAdd)
                {
                    //We must never have our own active and controlled vessel in the dictionary
                    if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel?.id == vesselToAdd.id)
                        continue;

                    FlightStatesDictionary.TryAdd(vesselToAdd.id, vesselToAdd.ctrlState);
                    FlyByWireDictionary.Add(vesselToAdd.id, st => LunaOnVesselFlyByWire(vesselToAdd.id, st));

                    vesselToAdd.OnFlyByWire += FlyByWireDictionary[vesselToAdd.id];
                }
            }
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
                    .Where(v => v != null && !v.OnFlyByWire.GetInvocationList().Any(d=> d.Method.Name == nameof(LunaOnVesselFlyByWire)));

                foreach (var vessel in vesselsToReasign)
                {
                    vessel.OnFlyByWire += FlyByWireDictionary[vessel.id];
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Here we copy the flight state we received and apply to the specific vessel.
        /// This method is called by ksp as it's a delegate
        /// </summary>
        private void LunaOnVesselFlyByWire(Guid id, FlightCtrlState st)
        {
            if (FlightStatesDictionary.TryGetValue(id, out var value))
            {
                st.CopyFrom(value);
            }
        }

        #endregion
    }
}
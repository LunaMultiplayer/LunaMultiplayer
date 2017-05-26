using LunaClient.Base;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselFlightStateSys
{
    /// <summary>
    /// System that controls the flight state (user control inputs) for all the vessesl that are in flight
    /// </summary>
    public class VesselFlightStateSystem : MessageSystem<VesselFlightStateSystem, VesselFlightStateMessageSender, VesselFlightStateMessageHandler>
    {
        #region Fields & properties

        private static Dictionary<Guid, FlightInputCallback> FlyByWireDictionary { get; } =
            new Dictionary<Guid, FlightInputCallback>();

        public Dictionary<Guid, FlightCtrlState> FlightStatesDictionary { get; } =
            new Dictionary<Guid, FlightCtrlState>();

        public bool FlightStateSystemReady
            => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
               FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
               FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
               FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, SendFlightState));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, RemoveUnloadedVesselsFromDictionary));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, AddRemoveActiveVesselFromDictionary));
            SetupRoutine(new RoutineDefinition(1500, RoutineExecution.Update, AddLoadedVesselsToDictionary));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            FlyByWireDictionary.Clear();
            FlightStatesDictionary.Clear();
        }

        #endregion

        #region Update methods

        private void SendFlightState()
        {
            if (Enabled && FlightStateSystemReady && VesselCommon.PlayerVesselsNearby())
            {
                //TODO: Don't we want to send the current flight state even if nobody is nearby--on some infrequent interval?
                MessageSender.SendCurrentFlightState();
            }

            if (Enabled)
            {
                ChangeRoutineExecutionInterval("SendFlightState", VesselCommon.IsSomeoneSpectatingUs ? 100 : 1000);
            }
        }

        private void RemoveUnloadedVesselsFromDictionary()
        {
            if (Enabled && FlightStateSystemReady)
            {
                var vesselsToRemove = FlightGlobals.VesselsUnloaded
                    .Where(v => FlyByWireDictionary.Keys.Contains(v.id))
                    .ToList();

                foreach (var vesselToRemove in vesselsToRemove)
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
                    FlightStatesDictionary.Remove(vesselToRemove.id);
                }
            }
        }

        private void AddRemoveActiveVesselFromDictionary()
        {
            if (Enabled && FlightStateSystemReady)
            {
                if (VesselCommon.IsSpectating)
                {
                    if (!FlyByWireDictionary.ContainsKey(FlightGlobals.ActiveVessel.id) &&
                        !FlightStatesDictionary.ContainsKey(FlightGlobals.ActiveVessel.id))
                    {
                        FlightStatesDictionary.Add(FlightGlobals.ActiveVessel.id, FlightGlobals.ActiveVessel.ctrlState);
                        FlyByWireDictionary.Add(FlightGlobals.ActiveVessel.id, st => OnVesselFlyByWire(FlightGlobals.ActiveVessel.id, st));
                        FlightGlobals.ActiveVessel.OnFlyByWire += FlyByWireDictionary[FlightGlobals.ActiveVessel.id];
                    }
                }
                else
                {
                    if (FlyByWireDictionary.ContainsKey(FlightGlobals.ActiveVessel.id) &&
                        FlightStatesDictionary.ContainsKey(FlightGlobals.ActiveVessel.id))
                    {
                        try
                        {
                            FlightGlobals.ActiveVessel.OnFlyByWire -= FlyByWireDictionary[FlightGlobals.ActiveVessel.id];
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        FlyByWireDictionary.Remove(FlightGlobals.ActiveVessel.id);
                        FlightStatesDictionary.Remove(FlightGlobals.ActiveVessel.id);
                    }
                }
            }
        }

        private void AddLoadedVesselsToDictionary()
        {
            if (Enabled && FlightStateSystemReady)
            {
                var vesselsToAdd = FlightGlobals.VesselsLoaded
                    .Where(v => v.id != FlightGlobals.ActiveVessel.id && !FlyByWireDictionary.Keys.Contains(v.id))
                    .ToArray();

                foreach (var vesselToAdd in vesselsToAdd)
                {
                    FlightStatesDictionary.Add(vesselToAdd.id, vesselToAdd.ctrlState);
                    FlyByWireDictionary.Add(vesselToAdd.id, st => OnVesselFlyByWire(vesselToAdd.id, st));
                    vesselToAdd.OnFlyByWire += FlyByWireDictionary[vesselToAdd.id];
                }
            }
        }

        #endregion

        #region Private methods

        private void OnVesselFlyByWire(Guid id, FlightCtrlState st)
        {
            if (FlightStatesDictionary.ContainsKey(id))
            {
                st.CopyFrom(FlightStatesDictionary[id]);
            }
        }

        #endregion
    }
}
using LunaClient.Base;
using System;
using System.Collections;
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
        private static Dictionary<Guid, FlightInputCallback> FlyByWireDictionary { get; } =
            new Dictionary<Guid, FlightInputCallback>();

        public Dictionary<Guid, FlightCtrlState> FlightStatesDictionary { get; } =
            new Dictionary<Guid, FlightCtrlState>();

        public bool FlightStateSystemReady
            => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
               FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
               FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
               FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        private const float DictionaryUpdateSInterval = 1.5f;
        private const float FlightStateSendSInterval = 0.1f;
        private const float FlightStateSendLowSInterval = 1f;

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendFlightState());
            Client.Singleton.StartCoroutine(AddRemoveActiveVesselFromDictionary());
            Client.Singleton.StartCoroutine(RemoveUnloadedVesselsFromDictionary());
            Client.Singleton.StartCoroutine(AddLoadedVesselsToDictionary());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            FlyByWireDictionary.Clear();
            FlightStatesDictionary.Clear();
        }

        private IEnumerator SendFlightState()
        {
            var seconds = new WaitForSeconds(FlightStateSendSInterval);
            var secondsFar = new WaitForSeconds(FlightStateSendLowSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady && VesselCommon.PlayerVesselsNearby())
                {
                    MessageSender.SendCurrentFlightState();
                }

                if(VesselCommon.IsSomeoneSpectatingUs)
                    yield return seconds;
                else
                    yield return secondsFar;
            }
        }

        private IEnumerator RemoveUnloadedVesselsFromDictionary()
        {
            var seconds = new WaitForSeconds(DictionaryUpdateSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady)
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

                yield return seconds;
            }
        }

        private IEnumerator AddRemoveActiveVesselFromDictionary()
        {
            var seconds = new WaitForSeconds(DictionaryUpdateSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady)
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

                yield return seconds;
            }
        }

        private IEnumerator AddLoadedVesselsToDictionary()
        {
            var seconds = new WaitForSeconds(DictionaryUpdateSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady)
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

                yield return seconds;
            }
        }

        private void OnVesselFlyByWire(Guid id, FlightCtrlState st)
        {
            if (FlightStatesDictionary.ContainsKey(id))
            {
                st.CopyFrom(FlightStatesDictionary[id]);
            }
        }
    }
}
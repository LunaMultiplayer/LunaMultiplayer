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
        /// <summary>
        /// Current flight state of the vessel we are spectating
        /// </summary>
        public FlightCtrlState FlightState { get; set; }

        private static bool _eventSet;

        public bool FlightStateSystemReady
            => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
               FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
               FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
               FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        private const float SetUnsetFlyByWireSInterval = 0.5f;
        private const float FlightStateSendSInterval = 0.1f;
        
        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendFlightState());
            Client.Singleton.StartCoroutine(SetUnsetFlyByWire());
        }
        
        private IEnumerator SendFlightState()
        {
            var seconds = new WaitForSeconds(FlightStateSendSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady)
                {
                    MessageSender.SendCurrentFlightState();
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Sets or unsets the callback for the flyby wire if we are spectating and release it when we are not spectating
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetUnsetFlyByWire()
        {
            var seconds = new WaitForSeconds(SetUnsetFlyByWireSInterval);
            while (true)
            {
                if (!Enabled) break;

                if (FlightStateSystemReady)
                {
                    if (VesselCommon.IsSpectating && !_eventSet)
                    {
                        FlightGlobals.ActiveVessel.OnFlyByWire += OnVesselFlyByWire;
                        _eventSet = true;
                    }
                    if (!VesselCommon.IsSpectating && _eventSet)
                    {
                        FlightGlobals.ActiveVessel.OnFlyByWire -= OnVesselFlyByWire;
                        _eventSet = false;
                    }
                }

                yield return seconds;
            }
        }
        
        private void OnVesselFlyByWire(FlightCtrlState st)
        {
            st.CopyFrom(FlightState);
        }
    }
}

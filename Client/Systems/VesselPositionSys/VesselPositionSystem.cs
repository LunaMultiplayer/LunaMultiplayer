using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using UnityEngine;


namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselPositionSystem : MessageSystem<VesselPositionSystem, VesselPositionMessageSender, VesselPositionMessageHandler>
    {
        #region Field & Properties

        private static float SecondaryVesselUpdatesSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval).TotalSeconds;

        private static float VesselUpdatesSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (PositionUpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        public Dictionary<Guid, Queue<VesselPositionUpdate>> ReceivedUpdates { get; } = new Dictionary<Guid, Queue<VesselPositionUpdate>>();

        private VesselPositionInterpolationSystem InterpolationSystem { get; } = new VesselPositionInterpolationSystem();

        public FlightCtrlState FlightState { get; set; }
        
        private static float _lastSentTime;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(InterpolationSystem.RemoveVessels());
            Client.Singleton.StartCoroutine(InterpolationSystem.AdjustInterpolationLengthFactor());
            Client.Singleton.StartCoroutine(SendSecondaryVesselPositionUpdates());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            InterpolationSystem.ResetSystem();
            ReceivedUpdates.Clear();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (PositionUpdateSystemReady)
            {
                InterpolationSystem.FixedUpdate();
                SendVesselPositionUpdates();
            }
        }

        #endregion

        #region Public methods
        
        public int GetNumberOfPositionUpdatesInQueue()
        {
            return ReceivedUpdates.Sum(u => u.Value.Count);
        }

        public int GetNumberOfPositionUpdatesInQueue(Guid vesselId)
        {
            return ReceivedUpdates[vesselId].Count;
        }

        /// <summary>
        /// Applies the current user's flight control state on the vessel's control state.  Used for spectated vessels.
        /// </summary>
        /// <param name="flightCtrlState"></param>
        public void ApplyFlightCtrlState(FlightCtrlState flightCtrlState)
        {
            if (FlightState != null)
            {
                flightCtrlState.CopyFrom(FlightState);
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Check if we must send a message or not based on the fixed time that has passed.
        /// Note that when no vessels are nearby or we are not in KSC the time is multiplied by 10
        /// </summary>
        private static bool ShouldSendPositionUpdate()
        {
            if (VesselCommon.IsSpectating) return false;

            var msSinceLastSend = TimeSpan.FromSeconds(Time.fixedTime - _lastSentTime).TotalMilliseconds;
            
            if (VesselCommon.PlayerVesselsNearby() || VesselCommon.IsNearKsc(20000))
            {
                return msSinceLastSend > VesselUpdatesSendSInterval;
            }

            return msSinceLastSend > VesselUpdatesSendSInterval * 10;
        }

        /// <summary>
        /// Send the updates of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && ShouldSendPositionUpdate())
            {
                _lastSentTime = Time.fixedTime;
                SendVesselPositionUpdate(FlightGlobals.ActiveVessel);
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock.
        /// </summary>
        private IEnumerator SendSecondaryVesselPositionUpdates()
        {
            var seconds = new WaitForSeconds(SecondaryVesselUpdatesSendSInterval);
            while (true)
            {
                if (!Enabled)
                    break;

                if (PositionUpdateSystemReady && ShouldSendPositionUpdate())
                {
                    var secondaryVesselsToUpdate = VesselCommon.GetSecondaryVessels();

                    foreach (var secondaryVessel in secondaryVesselsToUpdate)
                    {
                        SendVesselPositionUpdate(secondaryVessel);
                    }
                }

                yield return seconds;
            }
        }

        /// <summary>
        /// Create and send the vessel update
        /// </summary>
        private void SendVesselPositionUpdate(Vessel vessel)
        {
            var update = new VesselPositionUpdate(vessel);
            MessageSender.SendVesselPositionUpdate(update);
        }

        #endregion
    }
}

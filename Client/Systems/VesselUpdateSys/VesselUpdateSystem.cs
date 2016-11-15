using System;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using UnityEngine;


namespace LunaClient.Systems.VesselUpdateSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselUpdateSystem :
        MessageSystem<VesselUpdateSystem, VesselUpdateMessageSender, VesselUpdateMessageHandler>
    {
        #region Field & Properties

        private static float VesselUpdatesSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;

        private static float VesselUpdatesSendFarSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendFarMsInterval).TotalSeconds;

        public bool UpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool UpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (UpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        public Dictionary<Guid, Queue<VesselPositionUpdate>> ReceivedUpdates { get; } = new Dictionary<Guid, Queue<VesselPositionUpdate>>();

        private VesselUpdateInterpolationSystem InterpolationSystem { get; } = new VesselUpdateInterpolationSystem();

        public FlightCtrlState FlightState { get; set; }

        private long numUpdates = 0;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(InterpolationSystem.RemoveVessels());
            Client.Singleton.StartCoroutine(InterpolationSystem.AdjustInterpolationLengthFactor());
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
            if (UpdateSystemReady) {
                InterpolationSystem.FixedUpdate();
                SendVesselUpdates();
            }
        }

        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        #endregion

        #region Public methods

        public bool VesselHasUpdates(Guid vesselId, int minNumberOfUpdates)
        {
            return ReceivedUpdates.ContainsKey(vesselId) && ReceivedUpdates[vesselId].Count >= minNumberOfUpdates;
        }

        public int GetNumberOfUpdatesInQueue()
        {
            return ReceivedUpdates.Sum(u => u.Value.Count);
        }

        public int GetNumberOfUpdatesInQueue(Guid vesselId)
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
                //Interpolate the flight input state so the vessel controls look smooth
                flightCtrlState.CopyFrom(FlightState);
            }
        }

        #endregion

        #region Private methods

        private bool shouldSendUpdate()
        {
            numUpdates++;
            if (VesselCommon.PlayerVesselsNearby() || VesselCommon.isNearKSC(20000))
            {
                return (numUpdates % 6) == 0;
            }

            return (numUpdates % 50) == 0;
        }

        /// <summary>
        /// Send the updates of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselUpdates()
        {

            try
            {
                if (!Enabled)
                {
                    return;
                }

                if (shouldSendUpdate())
                {
                    if (UpdateSystemReady && !VesselCommon.IsSpectating)
                    {
                        
                        SendVesselUpdate(FlightGlobals.ActiveVessel);
                        //TODO: We should only send secondary vessels every few seconds or so.
                        //SendSecondaryVesselUpdates();
                        
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Coroutine error in SendVesselUpdates {e}");
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock.
        /// </summary>
        private void SendSecondaryVesselUpdates()
        {
            var secondaryVesselsToUpdate = VesselCommon.GetSecondaryVessels();

            foreach (var secondryVessel in secondaryVesselsToUpdate)
            {
                SendVesselUpdate(secondryVessel);
            }
        }

        /// <summary>
        /// Create and send the vessel update
        /// </summary>
        /// <param name="checkVessel"></param>
        private void SendVesselUpdate(Vessel checkVessel)
        {
            var update = VesselUpdate.CreateFromVessel(checkVessel);
            if (update != null)
            {
                MessageSender.SendVesselUpdate(update);
            }
            else
            {
                Debug.LogError("[LMP]: Cannot send vessel update!");
            }
        }

        #endregion
    }
}

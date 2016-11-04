using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselLockSys;
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
            (float) TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;

        private static float VesselUpdatesSendFarSInterval => 
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendFarMsInterval).TotalSeconds;

        public bool UpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag && !VesselCommon.ActiveVesselIsInSafetyBubble();

        public bool UpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (UpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        public Dictionary<Guid, Queue<VesselUpdate>> ReceivedUpdates { get; } = new Dictionary<Guid, Queue<VesselUpdate>>();

        private VesselUpdateInterpolationSystem InterpolationSystem { get; } = new VesselUpdateInterpolationSystem();

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(SendVesselUpdates());
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
            if (UpdateSystemReady)
                InterpolationSystem.FixedUpdate();

            base.FixedUpdate();
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

        #endregion

        #region Private methods

        /// <summary>
        /// Send the updates of our own vessel and the secondary vessels. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private IEnumerator SendVesselUpdates()
        {
            var seconds = new WaitForSeconds(VesselUpdatesSendSInterval);
            var secondsFar = new WaitForSeconds(VesselUpdatesSendFarSInterval);

            while (true)
            {
                try
                {
                    if (!Enabled)
                        break;

                    if (UpdateSystemReady && !VesselCommon.IsSpectating)
                    {
                        SendVesselUpdate(FlightGlobals.ActiveVessel);
                        SendSecondaryVesselUpdates();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in SendVesselUpdates {e}");
                }

                if (VesselCommon.PlayerVesselsNearby())
                    yield return seconds;
                else
                    yield return secondsFar;
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

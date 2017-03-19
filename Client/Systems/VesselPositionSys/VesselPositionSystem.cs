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
        private float lastSecondaryVesselSendTime = 0;

        private static float VesselUpdatesSendSInterval => (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;

        private const float MaxSecWithoutUpdates = 30;
        private const float RemoveVesselsSecInterval = 5;
        private float lastVesselRemovalTime = 0;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (PositionUpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        /// <summary>
        /// The queue of updates recieved from the network
        /// </summary>
        public Dictionary<Guid, Queue<VesselPositionUpdate>> ReceivedUpdates { get; } = new Dictionary<Guid, Queue<VesselPositionUpdate>>();

        public FlightCtrlState FlightState { get; set; }

        private static float _lastSentTime;

        /// <summary>
        /// The vessel update that are being handled for each vessel
        /// </summary>
        public Dictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselPositionUpdate>();

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            ResetSystem();
            ReceivedUpdates.Clear();
        }

        protected override bool HandleMessagesInFixedUpdate => true;

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (SettingsSystem.CurrentSettings.Debug2)
            {
                removeOldVessels();
            }

            if (PositionUpdateSystemReady)
            {
                handleReceivedUpdates();
                SendVesselPositionUpdates();
            }

            sendSecondaryVesselPositionUpdates();
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
        /// Remove a vessel from the system
        /// </summary>
        public void RemoveVessel(Guid vesselId)
        {
            ReceivedUpdates.Remove(vesselId);
            CurrentVesselUpdate.Remove(vesselId);
        }

        /// <summary>
        /// Clear all the properties
        /// </summary>
        public void ResetSystem()
        {
            CurrentVesselUpdate.Clear();
            ReceivedUpdates.Clear();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Main system that picks updates received and sets them for further processing. We call it in the 
        /// fixed update as in deals with physics
        /// </summary>
        private void handleReceivedUpdates()
        {
            HashSet<Guid> vesselsUpdated = new HashSet<Guid>();
            Profiler.BeginSample("vesselPositionUpdate");
            foreach (KeyValuePair<Guid, Queue<VesselPositionUpdate>> vesselInfo in ReceivedUpdates)
            {
                Guid vesselId = vesselInfo.Key;
                VesselPositionUpdate latestVesselUpdate = null;
                Queue<VesselPositionUpdate> vesselUpdateQueue = vesselInfo.Value;
                while (vesselUpdateQueue.Count > 0)
                {
                    VesselPositionUpdate vesselUpdate = vesselUpdateQueue.Dequeue();
                    if (latestVesselUpdate == null || latestVesselUpdate.SentTime < vesselUpdate.SentTime)
                    {
                        latestVesselUpdate = vesselUpdate;
                    }
                }

                if (latestVesselUpdate != null)
                {
                    HandleVesselUpdate(latestVesselUpdate);
                    vesselsUpdated.Add(vesselId);
                }
            }

            //Disable debug1 to stop vessel updates from being applied
            if (SettingsSystem.CurrentSettings.Debug1)
            {
                //Run through all the updates that are not finished and apply them
                foreach (Guid vesselId in vesselsUpdated)
                {
                    CurrentVesselUpdate[vesselId].applyVesselUpdate();

                }
            }
            Profiler.EndSample();
        }

        /// <summary>
        /// Handles a vessel update received from the network.  If the vessel update is later than any existing vessel updates for that vessel, updates the current update dictionary.
        /// </summary>
        /// <param name="vesselUpdate"></param>
        private void HandleVesselUpdate(VesselPositionUpdate vesselUpdate)
        {
            var Key = vesselUpdate.VesselId;
            if (!CurrentVesselUpdate.ContainsKey(Key) || CurrentVesselUpdate[Key].SentTime < vesselUpdate.SentTime)
            {
                CurrentVesselUpdate[Key] = vesselUpdate;
                vesselUpdate.initVesselUpdate();
            }
        }

        /// <summary>
        /// Remove the vessels that didn't receive and update after the value specified in MsWithoutUpdatesToRemove every 5 seconds
        /// </summary>
        private void removeOldVessels()
        {
            //Only remove vessels every RemoveVesselsSecInterval
            if ((Time.fixedTime - lastVesselRemovalTime) < RemoveVesselsSecInterval)
            {
                return;
            }
            lastVesselRemovalTime = Time.fixedTime;

            try
            {
                if (PositionUpdateSystemBasicReady)
                {
                    foreach (KeyValuePair<Guid, VesselPositionUpdate> vesselInfo in CurrentVesselUpdate)
                    {
                        Guid vesselId = vesselInfo.Key;
                        VesselPositionUpdate vesselUpdate = vesselInfo.Value;

                        //If our last update for this vessel was > 20 seconds ago and we have no pending vessel updates for this vessel, remove the vessel
                        if (vesselUpdate.ReceiveTime > MaxSecWithoutUpdates && !ReceivedUpdates.ContainsKey(vesselId))
                        {
                            RemoveVessel(vesselId);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[LMP]: Coroutine error in RemoveVessels {e}");
            }
        }

        /// <summary>
        /// Check if we must send a message or not based on the fixed time that has passed.
        /// Note that when no vessels are nearby or we are not in KSC the time is multiplied by 10
        /// </summary>
        private static bool ShouldSendPositionUpdate()
        {
            if (VesselCommon.IsSpectating)
            {
                return false;
            }

            var secSinceLastSend = Time.fixedTime - _lastSentTime;

            if (VesselCommon.PlayerVesselsNearby() || VesselCommon.IsNearKsc(20000))
            {
                return secSinceLastSend > VesselUpdatesSendSInterval;
            }

            return secSinceLastSend > VesselUpdatesSendSInterval * 10;
        }

        /// <summary>
        /// Send the updates of our own vessel. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && ShouldSendPositionUpdate())
            {
                _lastSentTime = Time.fixedTime;
                MessageSender.SendVesselPositionUpdate(FlightGlobals.ActiveVessel);
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock.
        /// </summary>
        private void sendSecondaryVesselPositionUpdates()
        {
            //Only send secondary vessel updates every SecondaryVesselUpdatesSendSInterval
            if ((Time.fixedTime - lastSecondaryVesselSendTime) < SecondaryVesselUpdatesSendSInterval)
            {
                return;
            }

            //If debug3 isn't enabled, don't send secondary vesselPositionUpdates
            if(!SettingsSystem.CurrentSettings.Debug3)
            {
                return;
            }

            lastSecondaryVesselSendTime = Time.fixedTime;

            if (PositionUpdateSystemReady && ShouldSendPositionUpdate())
            {
                var secondaryVesselsToUpdate = VesselCommon.GetSecondaryVessels();

                foreach (var secondaryVessel in secondaryVesselsToUpdate)
                {
                    MessageSender.SendVesselPositionUpdate(secondaryVessel);
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Interface;
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

        private static float VesselUpdatesSendSInterval => (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;

        private const float MaxSecWithoutUpdates = 20;
        private const float RemoveVesselsSecInterval = 5;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (PositionUpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        public ConcurrentDictionary<Guid, VesselPositionUpdate> ReceivedUpdates { get; } = 
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        private VesselPositionInterpolationSystem InterpolationSystem { get; } = new VesselPositionInterpolationSystem();

        public FlightCtrlState FlightState { get; set; }

        private static float _lastSentTime;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            Client.Singleton.StartCoroutine(RemoveVessels());
            Client.Singleton.StartCoroutine(SendSecondaryVesselPositionUpdates());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            InterpolationSystem.ResetSystem();
            ReceivedUpdates.Clear();
        }
        
        public override void Update()
        {
            base.Update();
            if (PositionUpdateSystemReady)
            {
                SendVesselPositionUpdates();
                InterpolationSystem.Update();
            }
        }

        /// <summary>
        /// This method is called on another thread so it won't affect the performance of KSP
        /// </summary>
        public override void EnqueueMessage(IMessageData msg)
        {
            if (Enabled)
            {
                MessageHandler.EnqueueNewMessage(msg);
            }
        }

        #endregion
        
        #region Private methods

        /// <summary>
        /// Remove the vessels that didn't receive and update after the value specified in MsWithoutUpdatesToRemove every 5 seconds
        /// </summary>
        private IEnumerator RemoveVessels()
        {
            var seconds = new WaitForSeconds(RemoveVesselsSecInterval);
            while (true)
            {
                try
                {
                    if (!Enabled) break;

                    if (PositionUpdateSystemBasicReady)
                    {
                        var vesselsToRemove = InterpolationSystem.CurrentVesselUpdate
                            .Where(u => u.Value.InterpolationFinished && Time.time - u.Value.FinishTime > MaxSecWithoutUpdates)
                            .Select(u => u.Key).ToArray();

                        foreach (var vesselId in vesselsToRemove)
                        {
                            InterpolationSystem.RemoveVessel(vesselId);
                            VesselPositionUpdate v;
                            ReceivedUpdates.TryRemove(vesselId, out v);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LMP]: Coroutine error in RemoveVessels {e}");
                }

                yield return seconds;
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
                        MessageSender.SendVesselPositionUpdate(secondaryVessel);
                    }
                }

                yield return seconds;
            }
        }

        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Vessel;
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

        public ConcurrentDictionary<Guid, VesselPositionUpdate> FutureVesselUpdate { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        public Dictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselPositionUpdate>();

        public FlightCtrlState FlightState { get; set; }

        private static float _lastSentTime;

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval, 
                RoutineExecution.Update, SendSecondaryVesselPositionUpdates));
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, ProcessPositions));
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
            FutureVesselUpdate.Clear();
        }

        public void ProcessPositions()
        {
            if (PositionUpdateSystemReady)
            {
                SendVesselPositionUpdates();
                HandleVesselUpdates();
            }
        }

        private void HandleVesselUpdates()
        {
            //Run to the new updates with vessels that are still not computed
            var newUpdates = FutureVesselUpdate.Where(v => !CurrentVesselUpdate.ContainsKey(v.Key)).ToList();
            foreach (var vesselUpdates in newUpdates)
            {
                SetFirstVesselUpdates(vesselUpdates.Value);
            }

            //Run trough all the vessels. Do this way or you get an out of sync in the dictionary!
            var vesselIds = CurrentVesselUpdate.Keys.ToList();
            foreach (var vesselId in vesselIds)
            {
                ProcessNewVesselUpdate(vesselId);
                CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
            }
        }

        private void ProcessNewVesselUpdate(Guid vesselId)
        {
            if (CurrentVesselUpdate[vesselId].Id == FutureVesselUpdate[vesselId].Id)
            {
                //No message to process!!
                return;
            }

            FutureVesselUpdate[vesselId].Vessel = CurrentVesselUpdate[vesselId].Vessel;
            if (FutureVesselUpdate[vesselId].BodyName == CurrentVesselUpdate[vesselId].BodyName)
                FutureVesselUpdate[vesselId].Body = CurrentVesselUpdate[vesselId].Body;

            CurrentVesselUpdate[vesselId] = CurrentVesselUpdate[vesselId].Target;
            CurrentVesselUpdate[vesselId].Target = FutureVesselUpdate[vesselId];
        }

        /// <summary>
        /// Here we set the first vessel updates. We use the current vessel state as the starting point.
        /// </summary>
        private void SetFirstVesselUpdates(VesselPositionUpdate update)
        {
            var first = update?.Clone();
            if (first != null)
            {
                first.SentTime = update.SentTime;
                update.SentTime += 0.5f;

                CurrentVesselUpdate.Add(update.VesselId, first);
                CurrentVesselUpdate[update.VesselId].Target = update;
            }
        }

        /// <summary>
        /// Do the message handling asynchronously for performance
        /// </summary>
        public override void EnqueueMessage(IMessageData msg)
        {
            if (Enabled)
            {
                new Thread(() =>
                {
                    var msgData = msg as VesselPositionMsgData;
                    if (msgData == null)
                    {
                        return;
                    }

                    var update = new VesselPositionUpdate(msgData);
                    if (!FutureVesselUpdate.ContainsKey(update.VesselId))
                    {
                        FutureVesselUpdate.TryAdd(update.VesselId, update);
                    }
                    else
                    {
                        if (FutureVesselUpdate[update.VesselId].SentTime < update.SentTime)
                        {
                            FutureVesselUpdate[update.VesselId] = update;
                        }
                    }
                }).Start();
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
        private void SendSecondaryVesselPositionUpdates()
        {
            if (Enabled && PositionUpdateSystemReady && ShouldSendPositionUpdate())
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

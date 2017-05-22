using System;
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
        #region Constructor

        public VesselPositionSystem()
        {
            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, ProcessPositions));
        }

        #endregion

        #region Field & Properties

        private static float SecondaryVesselUpdatesSendSInterval =>
            (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval).TotalSeconds;

        private static float VesselUpdatesSendSInterval => (float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval).TotalSeconds;
        
        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            (PositionUpdateSystemReady) || (HighLogic.LoadedScene == GameScenes.TRACKSTATION);

        public ConcurrentDictionary<Guid, VesselPositionUpdate> FutureVesselUpdates { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        public Dictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new Dictionary<Guid, VesselPositionUpdate>();

        public FlightCtrlState FlightState { get; set; }
        
        private static float _lastSentTime;

        #endregion

        #region Base overrides

        public override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
            FutureVesselUpdates.Clear();
        }
        
        #endregion

        #region Fixed update methods

        private void ProcessPositions()
        {
            if (PositionUpdateSystemReady)
            {
                HandleVesselUpdates();
                SendVesselPositionUpdates();
                SendSecondaryVesselPositionUpdates();
            }
        }

        #endregion

        #region Private methods

        private void HandleVesselUpdates()
        {
            HashSet<Guid> vesselIdsUpdated = new HashSet<Guid>();

            //Run to the new updates with vessels that are still not computed
            foreach (KeyValuePair<Guid, VesselPositionUpdate> entry in FutureVesselUpdates)
            {
                Guid vesselId = entry.Key;
                VesselPositionUpdate positionUpdate;
                FutureVesselUpdates.TryRemove(vesselId, out positionUpdate);

                if (positionUpdate != null)
                {
                    setBodyOnNewVesselUpdate(vesselId, positionUpdate);
                    CurrentVesselUpdate[vesselId] = positionUpdate;

                    //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've processed this.
                    vesselIdsUpdated.Add(vesselId);
                }
            }

            //NOTE: The below code must run in FixedUpdate.  However, the previous code could run outside fixedUpdate, potentially even in another thread.
            foreach (Guid vesselId in vesselIdsUpdated)
            {
                CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
            }
        }

        /// <summary>
        /// Performance Improvement: If the body for this position update is the same as the old one, copy the body so that we don't have to look up the
        /// body in the ApplyVesselUpdate() call below (which must run during FixedUpdate)
        /// </summary>
        /// <param name="vesselId">The ID of the vessel being updated</param>
        /// <param name="newPositionUpdate">The new position update for the vessel.  Cannot be null</param>
        private void setBodyOnNewVesselUpdate(Guid vesselId, VesselPositionUpdate newPositionUpdate)
        {
            VesselPositionUpdate existingPositionUpdate;
            CurrentVesselUpdate.TryGetValue(vesselId, out existingPositionUpdate);
            if (existingPositionUpdate != null && newPositionUpdate.BodyName == existingPositionUpdate.BodyName)
            {
                newPositionUpdate.Body = existingPositionUpdate.Body;
            }
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Applies the latest position update (if any) for the given vessel and moves it to that position
        /// </summary>
        /// <param name="VesselId"></param>
        public void updateVesselPosition(Guid vesselId)
        {
            if (PositionUpdateSystemReady)
            {
                if (CurrentVesselUpdate.ContainsKey(vesselId))
                {
                    CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
                }
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
                    if (!FutureVesselUpdates.ContainsKey(update.VesselId))
                    {
                        FutureVesselUpdates.TryAdd(update.VesselId, update);
                    }
                    else
                    {
                        if (FutureVesselUpdates[update.VesselId].SentTime < update.SentTime)
                        {
                            FutureVesselUpdates[update.VesselId] = update;
                        }
                    }
                }).Start();
            }
        }

        #endregion

        #region Private methods 2
        
        /// <summary>
        /// Check if we must send a message or not based on the fixed time that has passed.
        /// Note that when no vessels are nearby or we are not in KSC the time is multiplied by 10
        /// //TODO: Should change this to use a timer in MessageSystem
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
        /// TODO: Who calls this method now?  How do we make sure we only infrequently send updates for secondary vessels, as opposed to primary vessels?
        private void SendSecondaryVesselPositionUpdates()
        {
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

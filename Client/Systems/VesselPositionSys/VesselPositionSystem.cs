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

        private ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new ConcurrentDictionary<Guid, VesselPositionUpdate>();
        private ConcurrentDictionary<Guid, byte> updatedVesselIds { get; } = new ConcurrentDictionary<Guid, byte>();
        

        public FlightCtrlState FlightState { get; set; }
        
        private static float _lastSentTime;

        #endregion

        #region Base overrides

        public override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
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
            //NOTE: The below code must run in FixedUpdate.  However, the previous code could run outside fixedUpdate, potentially even in another thread.
            foreach (Guid vesselId in updatedVesselIds.Keys)
            {
                CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
            }
        }

        /// <summary>
        /// Performance Improvement: If the body for this position update is the same as the old one, copy the body so that we don't have to look up the
        /// body in the ApplyVesselUpdate() call below (which must run during FixedUpdate)
        /// </summary>
        /// <param name="existingPositionUpdate">The position update currently in the map.  Cannot be null.</param>
        /// <param name="newPositionUpdate">The new position update for the vessel.  Cannot be null.</param>
        private void setBodyAndVesselOnNewUpdate(VesselPositionUpdate existingPositionUpdate, VesselPositionUpdate newPositionUpdate)
        {
            if (existingPositionUpdate.BodyName == newPositionUpdate.BodyName)
            {
                newPositionUpdate.Body = existingPositionUpdate.Body;
            }
            if (existingPositionUpdate.VesselId == newPositionUpdate.VesselId)
            {
                newPositionUpdate.Vessel = existingPositionUpdate.Vessel;
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
                    Guid vesselId = update.VesselId;

                    VesselPositionUpdate existingPositionUpdate;
                    if (!CurrentVesselUpdate.TryGetValue(update.VesselId, out existingPositionUpdate))
                    {
                        CurrentVesselUpdate[vesselId] = update;
                        //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                        updatedVesselIds[vesselId] = 0;
                    }
                    else
                    {
                        if (existingPositionUpdate.SentTime < update.SentTime)
                        {
                            //If there's an existing update, copy the body and vessel objects so they don't have to be looked up later.
                            setBodyAndVesselOnNewUpdate(existingPositionUpdate, update);
                            CurrentVesselUpdate[vesselId] = update;

                            //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                            updatedVesselIds[vesselId] = 0;
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

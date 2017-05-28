using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Interface;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionSys
{
    /// <summary>
    /// System that handle the received vessel update messages and also sends them
    /// </summary>
    public class VesselPositionSystem : MessageSystem<VesselPositionSystem, VesselPositionMessageSender, VesselPositionMessageHandler>
    {
        #region Fields & properties

        private static bool MustSendFastUpdates => VesselCommon.PlayerVesselsNearby() || VesselCommon.IsNearKsc(20000);
        private static int FastVesselUpdatesSendMsInterval => SettingsSystem.ServerSettings.VesselUpdatesSendMsInterval;
        private static int SlowVesselUpdatesSendMsInterval => FastVesselUpdatesSendMsInterval * 5;

        public bool PositionUpdateSystemReady => Enabled && FlightGlobals.ActiveVessel != null && Time.timeSinceLevelLoad > 1f &&
                                         FlightGlobals.ready && FlightGlobals.ActiveVessel.loaded &&
                                         FlightGlobals.ActiveVessel.state != Vessel.State.DEAD && !FlightGlobals.ActiveVessel.packed &&
                                         FlightGlobals.ActiveVessel.vesselType != VesselType.Flag;

        public bool PositionUpdateSystemBasicReady => Enabled && Time.timeSinceLevelLoad > 1f &&
            PositionUpdateSystemReady || HighLogic.LoadedScene == GameScenes.TRACKSTATION;

        private ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new ConcurrentDictionary<Guid, VesselPositionUpdate>();
        private ConcurrentDictionary<Guid, byte> UpdatedVesselIds { get; } = new ConcurrentDictionary<Guid, byte>();
        public FlightCtrlState FlightState { get; set; }

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, HandleVesselUpdates));

            SetupRoutine(new RoutineDefinition(FastVesselUpdatesSendMsInterval,
                RoutineExecution.FixedUpdate, SendVesselPositionUpdates));

            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval,
                RoutineExecution.FixedUpdate, SendSecondaryVesselPositionUpdates));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
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
                    var vesselId = update.VesselId;

                    if (!CurrentVesselUpdate.TryGetValue(update.VesselId, out var existingPositionUpdate))
                    {
                        CurrentVesselUpdate[vesselId] = update;
                        //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                        UpdatedVesselIds[vesselId] = 0;
                    }
                    else
                    {
                        if (existingPositionUpdate.SentTime < update.SentTime)
                        {
                            //If there's an existing update, copy the body and vessel objects so they don't have to be looked up later.
                            SetBodyAndVesselOnNewUpdate(existingPositionUpdate, update);
                            CurrentVesselUpdate[vesselId] = update;

                            //If we got a position update, add it to the vessel IDs updated and the current vessel dictionary, after we've added it to the CurrentVesselUpdate dictionary
                            UpdatedVesselIds[vesselId] = 0;
                        }
                    }
                }).Start();
            }
        }

        #endregion

        #region FixedUpdate methods

        /// <summary>
        /// Read and apply position updates from other vessels
        /// </summary>
        private void HandleVesselUpdates()
        {
            if (PositionUpdateSystemReady)
            {
                foreach (var vesselId in UpdatedVesselIds.Keys)
                {
                    //NOTE: ApplyVesselUpdate must run in FixedUpdate as it's updating the physics of the vessels
                    CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
                    UpdatedVesselIds.TryRemove(vesselId, out var _);
                }
            }
        }

        /// <summary>
        /// Send the updates of our own vessel. We only send them after an interval specified.
        /// If the other player vessels are far we don't send them very often.
        /// </summary>
        private void SendVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && !VesselCommon.IsSpectating)
            {
                MessageSender.SendVesselPositionUpdate(FlightGlobals.ActiveVessel);
                ChangeRoutineExecutionInterval("SendVesselPositionUpdates",
                    MustSendFastUpdates ? FastVesselUpdatesSendMsInterval : SlowVesselUpdatesSendMsInterval);
            }
        }

        /// <summary>
        /// Send updates for vessels that we own the update lock.
        /// </summary>
        private void SendSecondaryVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && !VesselCommon.IsSpectating)
            {
                var secondaryVesselsToUpdate = VesselCommon.GetSecondaryVessels();
                foreach (var secondaryVessel in secondaryVesselsToUpdate)
                {
                    MessageSender.SendVesselPositionUpdate(secondaryVessel);
                }
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Applies the latest position update (if any) for the given vessel and moves it to that position
        /// </summary>
        public void UpdateVesselPosition(Guid vesselId)
        {
            if (PositionUpdateSystemReady && CurrentVesselUpdate.ContainsKey(vesselId))
            {
                CurrentVesselUpdate[vesselId].ApplyVesselUpdate();
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Performance Improvement: If the body for this position update is the same as the old one, copy the body so that we don't have to look up the
        /// body in the ApplyVesselUpdate() call below (which must run during FixedUpdate)
        /// </summary>
        /// <param name="existingPositionUpdate">The position update currently in the map.  Cannot be null.</param>
        /// <param name="newPositionUpdate">The new position update for the vessel.  Cannot be null.</param>
        private static void SetBodyAndVesselOnNewUpdate(VesselPositionUpdate existingPositionUpdate, VesselPositionUpdate newPositionUpdate)
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
    }
}

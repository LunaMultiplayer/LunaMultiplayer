using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace LunaClient.Systems.VesselPositionAltSys
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

        public ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } = new ConcurrentDictionary<Guid, VesselPositionUpdate>();
        public ConcurrentDictionary<Guid, byte> UpdatedVesselIds { get; } = new ConcurrentDictionary<Guid, byte>();
        public FlightCtrlState FlightState { get; set; }

        #endregion

        #region Base overrides

        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();

            SetupRoutine(new RoutineDefinition(0, RoutineExecution.FixedUpdate, HandleVesselUpdates));

            SetupRoutine(new RoutineDefinition(FastVesselUpdatesSendMsInterval,
                RoutineExecution.Update, SendVesselPositionUpdates));

            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval,
                RoutineExecution.Update, SendSecondaryVesselPositionUpdates));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
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
                    if (CurrentVesselUpdate.TryGetValue(vesselId, out var update))
                    {
                        update.ApplyVesselUpdate();
                        UpdatedVesselIds.TryRemove(vesselId, out var _);
                    }
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
        /// Performance Improvement: If the body for this position update is the same as the old one, copy the body so that we don't have to look up the
        /// body in the ApplyVesselUpdate() call below (which must run during FixedUpdate)
        /// </summary>
        /// <param name="existingPositionUpdate">The position update currently in the map.  Cannot be null.</param>
        /// <param name="newPositionUpdate">The new position update for the vessel.  Cannot be null.</param>
        public void SetBodyAndVesselOnNewUpdate(VesselPositionUpdate existingPositionUpdate, VesselPositionUpdate newPositionUpdate)
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
    }
}

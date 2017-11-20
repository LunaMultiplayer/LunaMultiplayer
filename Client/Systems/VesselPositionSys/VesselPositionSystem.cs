using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public static ConcurrentDictionary<Guid, VesselPositionUpdate> CurrentVesselUpdate { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        public static ConcurrentDictionary<Guid, VesselPositionUpdate> TargetVesselUpdate { get; } =
            new ConcurrentDictionary<Guid, VesselPositionUpdate>();

        public static Queue<Guid> VesselsToRemove { get; } = new Queue<Guid>();

        #endregion

        #region Base overrides
        
        protected override bool ProcessMessagesInUnityThread => false;

        protected override void OnEnabled()
        {
            base.OnEnabled();
            
            TimingManager.FixedUpdateAdd(TimingManager.TimingStage.ObscenelyEarly, HandleVesselUpdates);

            SetupRoutine(new RoutineDefinition(FastVesselUpdatesSendMsInterval, RoutineExecution.LateUpdate, SendVesselPositionUpdates));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval,
                RoutineExecution.Update, SendSecondaryVesselPositionUpdates));
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.SecondaryVesselUpdatesSendMsInterval * 2,
                RoutineExecution.Update, SendUnloadedSecondaryVesselPositionUpdates));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            CurrentVesselUpdate.Clear();
            
            TimingManager.FixedUpdateRemove(TimingManager.TimingStage.ObscenelyEarly, HandleVesselUpdates);
        }
        
        private static void HandleVesselUpdates()
        {
            foreach (var keyVal in CurrentVesselUpdate)
            {
                keyVal.Value.ApplyVesselUpdate();
            }

            while (VesselsToRemove.Count > 0)
            {
                var vesselToRemove = VesselsToRemove.Dequeue();
                TargetVesselUpdate.TryRemove(vesselToRemove, out _);
                CurrentVesselUpdate.TryRemove(vesselToRemove, out _);
            }
        }

        #endregion

        #region FixedUpdate methods

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
        /// Send updates for vessels that we own the update lock. And also send it for the abandoned ones
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

        /// <summary>
        /// Send updates for vessels that we own the update lock. And also send it for the abandoned ones
        /// </summary>
        private void SendUnloadedSecondaryVesselPositionUpdates()
        {
            if (PositionUpdateSystemReady && !VesselCommon.IsSpectating)
            {
                var abandonedVesselsToUpdate = VesselCommon.GetUnloadedSecondaryVessels();
                foreach (var secondaryVessel in abandonedVesselsToUpdate)
                {
                    MessageSender.SendVesselPositionUpdate(secondaryVessel);
                }
            }
        }

        #endregion

        /// <summary>
        /// Gets the latest received position of a vessel
        /// </summary>
        public double[] GetLatestVesselPosition(Guid vesselId)
        {
            return TargetVesselUpdate.TryGetValue(vesselId, out var vesselPosition) ? 
                vesselPosition.LatLonAlt :
                CurrentVesselUpdate.TryGetValue(vesselId, out vesselPosition) ?
                    vesselPosition.LatLonAlt :
                    null;
        }
    }
}

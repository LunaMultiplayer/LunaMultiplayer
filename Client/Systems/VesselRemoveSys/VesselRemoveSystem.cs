using KSP.UI.Screens;
using LunaClient.Base;
using LunaClient.Events;
using LunaClient.Localization;
using LunaClient.VesselUtilities;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using UniLinq;

namespace LunaClient.Systems.VesselRemoveSys
{
    /// <summary>
    /// This system handles the killing of vessels. We kill the vessels that are not in our subspace and 
    /// the vessels that are destroyed, old copies of changed vessels or when they dock
    /// </summary>
    public class VesselRemoveSystem : MessageSystem<VesselRemoveSystem, VesselRemoveMessageSender, VesselRemoveMessageHandler>
    {
        private class VesselRemoveEntry
        {
            public Guid VesselId { get; }
            public string Reason { get; }

            public VesselRemoveEntry(Guid vesselId, string reason)
            {
                VesselId = vesselId;
                Reason = "Queued: " + reason;
            }
        }

        #region Fields & properties

        private VesselRemoveEvents VesselRemoveEvents { get; } = new VesselRemoveEvents();
        private static ConcurrentQueue<VesselRemoveEntry> VesselsToRemove { get; set; } = new ConcurrentQueue<VesselRemoveEntry>();
        private static ConcurrentDictionary<Guid, DateTime> RemovedVessels { get; } = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselRemoveSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselWillDestroy.Add(VesselRemoveEvents.OnVesselWillDestroy);

            RevertEvent.onRevertedToLaunch.Add(VesselRemoveEvents.OnRevertToLaunch);
            RevertEvent.onRevertedToPrelaunch.Add(VesselRemoveEvents.OnRevertToEditor);
            RevertEvent.onReturnedToEditor.Add(VesselRemoveEvents.OnRevertToEditor);

            SetupRoutine(new RoutineDefinition(500, RoutineExecution.Update, RemoveQueuedVessels));
            SetupRoutine(new RoutineDefinition(20000, RoutineExecution.Update, FlushRemovedVessels));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            ClearSystem();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselWillDestroy.Remove(VesselRemoveEvents.OnVesselWillDestroy);

            RevertEvent.onRevertedToLaunch.Remove(VesselRemoveEvents.OnRevertToLaunch);
            RevertEvent.onRevertedToPrelaunch.Remove(VesselRemoveEvents.OnRevertToEditor);
            RevertEvent.onReturnedToEditor.Remove(VesselRemoveEvents.OnRevertToEditor);
        }

        #endregion

        #region Public

        /// <summary>
        /// Clears the dictionary, you should call this method when switching scene
        /// </summary>
        public void ClearSystem()
        {
            VesselsToRemove = new ConcurrentQueue<VesselRemoveEntry>();
            RemovedVessels.Clear();
        }

        /// <summary>
        /// Add a vessel so it will be killed later
        /// </summary>
        public void AddToKillList(Guid vesselId, string reason)
        {
            VesselsToRemove.Enqueue(new VesselRemoveEntry(vesselId, reason));
        }

        /// <summary>
        /// Check if vessel is in the kill list
        /// </summary>
        public bool VesselWillBeKilled(Guid vesselId)
        {
            return VesselsToRemove.Any(v => v.VesselId == vesselId) || RemovedVessels.ContainsKey(vesselId);
        }

        /// <summary>
        /// Kills a vessel.
        /// </summary>
        public void KillVessel(Guid vesselId, string reason)
        {
            VesselCommon.RemoveVesselFromSystems(vesselId);

            var killVessel = FlightGlobals.FindVessel(vesselId);
            if (killVessel == null || killVessel.state == Vessel.State.DEAD)
                return;

            LunaLog.Log($"[LMP]: Killing vessel {killVessel.id}. Reason: {reason}");
            SwitchVesselIfKillingActiveVessel(killVessel);

            try
            {
                if (FlightGlobals.fetch.VesselTarget?.GetVessel().id == killVessel.id)
                {
                    FlightGlobals.fetch.SetVesselTarget(null);
                }

                FlightGlobals.RemoveVessel(killVessel);
                foreach (var part in killVessel.parts)
                {
                    UnityEngine.Object.Destroy(part.gameObject);
                }
                UnityEngine.Object.Destroy(killVessel.gameObject);

                HighLogic.CurrentGame.flightState.protoVessels.RemoveAll(v => v == null || v.vesselID == killVessel.id);
                KSCVesselMarkers.fetch?.RefreshMarkers();
            }
            catch (Exception killException)
            {
                LunaLog.LogError($"[LMP]: Error destroying vessel: {killException}");
            }
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Flush vessels older than 20 seconds
        /// </summary>
        private void FlushRemovedVessels()
        {
            var vesselsToFlush = RemovedVessels
                .Where(v => (LunaNetworkTime.UtcNow - v.Value) > TimeSpan.FromSeconds(20))
                .Select(v => v.Key);

            foreach (var vesselId in vesselsToFlush)
            {
                RemovedVessels.TryRemove(vesselId, out _);
            }
        }

        /// <summary>
        /// Unload or kills the vessels in the queue
        /// </summary>
        private void RemoveQueuedVessels()
        {
            while (VesselsToRemove.TryDequeue(out var vesselRemoveEntry))
            {
                KillVessel(vesselRemoveEntry.VesselId, vesselRemoveEntry.Reason);

                //Always add to the killed list even if it exists that vessel or not.
                RemovedVessels.TryAdd(vesselRemoveEntry.VesselId, LunaNetworkTime.UtcNow);
            }
        }

        #endregion

        #region Private methods

        private static void SwitchVesselIfKillingActiveVessel(Vessel killVessel)
        {
            if (FlightGlobals.ActiveVessel?.id == killVessel.id)
            {
                FlightGlobals.fetch.SetVesselTarget(null);

                //Try to switch to a nearby loaded vessel...
                var otherVessel = FlightGlobals.FindNearestControllableVessel(killVessel);
                if (otherVessel != null)
                    FlightGlobals.ForceSetActiveVessel(otherVessel);
                else
                    HighLogic.LoadScene(GameScenes.SPACECENTER);

                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.SpectatingRemoved, 10f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        #endregion
    }
}

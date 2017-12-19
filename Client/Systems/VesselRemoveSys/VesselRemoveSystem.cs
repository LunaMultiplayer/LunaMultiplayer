using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
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
        #region Fields & properties

        private VesselRemoveEvents VesselRemoveEvents { get; } = new VesselRemoveEvents();

        public ConcurrentQueue<Guid> VesselsToRemove { get; private set; } = new ConcurrentQueue<Guid>();
        public ConcurrentDictionary<Guid, DateTime> RemovedVessels { get; } = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Base overrides

        public override string SystemName { get; } = nameof(VesselRemoveSystem);

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselWillDestroy.Add(VesselRemoveEvents.OnVesselWillDestroy);
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, KillPastSubspaceVessels));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveQueuedVessels));
            SetupRoutine(new RoutineDefinition(20000, RoutineExecution.Update, FlushRemovedVessels));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselsToRemove = new ConcurrentQueue<Guid>();
            RemovedVessels.Clear();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselWillDestroy.Remove(VesselRemoveEvents.OnVesselWillDestroy);
        }

        #endregion

        #region Public

        /// <summary>
        /// Clears the dictionary, you should call this method when switching scene
        /// </summary>
        public void ClearSystem()
        {
            VesselsToRemove = new ConcurrentQueue<Guid>();
            RemovedVessels.Clear();
        }

        /// <summary>
        /// Add a vessel so it will be killed later
        /// </summary>
        public void AddToKillList(Guid vesselId)
        {
            VesselsToRemove.Enqueue(vesselId);
            VesselsProtoStore.RemoveVessel(vesselId);
        }

        /// <summary>
        /// Check if vessel is in the kill list
        /// </summary>
        public bool VesselWillBeKilled(Guid vesselId)
        {
            return VesselsToRemove.Contains(vesselId) || RemovedVessels.ContainsKey(vesselId);
        }

        /// <summary>
        /// Unloads a vessel from the game in 1 frame. Caution with this method as it can generate issues!
        /// Specially if you receive a message for a vessel and that vessel is not found as you called this method
        /// </summary>
        public void UnloadVessel(Vessel killVessel)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel))
            {
                return;
            }
            
            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);
        }

        /// <summary>
        /// Unloads a vessel from the game in 1 frame. Caution with this method as it can generate issues!
        /// Specially if you receive a message for a vessel and that vessel is not found as you called this method
        /// </summary>
        public void UnloadVessel(Guid vesselId)
        {
            UnloadVessel(FlightGlobals.FindVessel(vesselId));
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Flush vessels older than 20 seconds
        /// </summary>
        private void FlushRemovedVessels()
        {
            var vesselsToFlush = RemovedVessels
                .Where(v => (LunaTime.Now - v.Value) > TimeSpan.FromSeconds(20))
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
            while(VesselsToRemove.TryDequeue(out var vesselId))
            {
                KillVessel(vesselId);

                RemovedVessels.TryAdd(vesselId, LunaTime.Now);
                VesselsProtoStore.RemoveVessel(vesselId);
            }

            VesselsToRemove = new ConcurrentQueue<Guid>();
        }

        /// <summary>
        /// Get the vessels that are in a past subspace and kill them
        /// </summary>
        private void KillPastSubspaceVessels()
        {
            if (SettingsSystem.ServerSettings.ShowVesselsInThePast) return;

            if (Enabled)
            {
                var vesselsToUnloadIds = VesselsProtoStore.AllPlayerVessels
                    .Where(v => v.Value.VesselExist && VesselCommon.VesselIsControlledAndInPastSubspace(v.Key));

                foreach (var vesselId in vesselsToUnloadIds)
                {
                    AddToKillList(vesselId.Key);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Kills and unloads a vessel.
        /// </summary>
        private static void KillVessel(Guid vesselId)
        {
            var killVessel = FlightGlobals.FindVessel(vesselId);
            if (killVessel == null || killVessel.state == Vessel.State.DEAD)
                return;

            LunaLog.Log($"[LMP]: Killing vessel {killVessel.id}");

            SwitchVesselIfSpectating(killVessel);
            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);
        }

        private static void SwitchVesselIfSpectating(Vessel killVessel)
        {
            if (FlightGlobals.ActiveVessel?.id == killVessel.id)
            {
                //Get a random vessel and switch to it if exists, otherwise go to spacecenter
                var otherVessel = FlightGlobals.Vessels.FirstOrDefault(v => v.id != killVessel.id);
                
                if (otherVessel != null)
                    FlightGlobals.ForceSetActiveVessel(otherVessel);
                else
                    HighLogic.LoadScene(GameScenes.SPACECENTER);

                ScreenMessages.PostScreenMessage("The vessel you were spectating was removed", 10f);
            }
        }

        /// <summary>
        /// Removes the vessel from the scenario. 
        /// If you don't call this, the vessel will still be found in FlightGlobals.Vessels
        /// </summary>
        private static void UnloadVesselFromScenario(Vessel killVessel)
        {
            try
            {
                HighLogic.CurrentGame.DestroyVessel(killVessel);
                HighLogic.CurrentGame.Updated();
            }
            catch (Exception destroyException)
            {
                LunaLog.LogError($"[LMP]: Error destroying vessel from the scenario: {destroyException}");
            }
        }

        /// <summary>
        /// Kills the vessel
        /// </summary>
        private static void KillGivenVessel(Vessel killVessel)
        {
            try
            {
                //This method will call our event "OnVesselWillDestroy" and remove the locks if needed...
                killVessel?.Die();
            }
            catch (Exception killException)
            {
                LunaLog.LogError($"[LMP]: Error destroying vessel: {killException}");
            }
        }

        /// <summary>
        /// Unload the vessel so the crew is kiled when we remove the vessel.
        /// </summary>
        private static void UnloadVesselFromGame(Vessel killVessel)
        {
            if (killVessel.loaded)
            {
                try
                {
                    killVessel.Unload();
                }
                catch (Exception unloadException)
                {
                    LunaLog.LogError($"[LMP]: Error unloading vessel: {unloadException}");
                }
            }
        }

        #endregion
    }
}

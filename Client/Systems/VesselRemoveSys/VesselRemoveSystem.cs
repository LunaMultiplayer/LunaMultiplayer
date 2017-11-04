using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
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

        public ConcurrentDictionary<Guid, Vessel> VesselsToRemove { get; } = new ConcurrentDictionary<Guid, Vessel>();
        public ConcurrentDictionary<Guid, DateTime> RemovedVessels { get; } = new ConcurrentDictionary<Guid, DateTime>();

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Add(VesselRemoveEvents.OnVesselDestroyed);
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, KillPastSubspaceVessels));
            SetupRoutine(new RoutineDefinition(1000, RoutineExecution.Update, RemoveQueuedVessels));
            SetupRoutine(new RoutineDefinition(20000, RoutineExecution.Update, FlushRemovedVessels));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            VesselsToRemove.Clear();
            RemovedVessels.Clear();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Remove(VesselRemoveEvents.OnVesselDestroyed);
        }

        #endregion

        #region Public

        /// <summary>
        /// Clears the dictionary, you should call this method when switching scene
        /// </summary>
        public void ClearSystem()
        {
            VesselsToRemove.Clear();
            RemovedVessels.Clear();
        }

        /// <summary>
        /// Add a vessel so it will be killed later
        /// </summary>
        public void AddToKillList(Vessel vessel)
        {
            if (vessel == null) return;

            VesselsToRemove.TryAdd(vessel.id, vessel);
            SystemsContainer.Get<VesselProtoSystem>().RemoveVesselFromLoadingSystem(vessel.id);
        }

        /// <summary>
        /// Check if vessel is in the kill list
        /// </summary>
        public bool VesselWillBeKilled(Guid vesselId)
        {
            return VesselsToRemove.ContainsKey(vesselId) || RemovedVessels.ContainsKey(vesselId);
        }

        /// <summary>
        /// Unloads a vessel from the game in 1 frame. Caution with this method as it can generate issues!
        /// Specially if you receive a message for a vessel and that vessel is not found as you called this method
        /// </summary>
        public void UnloadVessel(Vessel killVessel)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel) || killVessel.state == Vessel.State.DEAD)
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
                .Where(v => (DateTime.Now - v.Value) > TimeSpan.FromSeconds(20))
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
            foreach (var vessel in VesselsToRemove.Values)
            {
                KillVessel(vessel);
                RemovedVessels.TryAdd(vessel.id, DateTime.Now);
                SystemsContainer.Get<VesselProtoSystem>().RemoveVesselFromLoadingSystem(vessel.id);
            }
            VesselsToRemove.Clear();
        }

        /// <summary>
        /// Get the vessels that are in a past subspace and kill them
        /// </summary>
        private void KillPastSubspaceVessels()
        {
            if (SettingsSystem.ServerSettings.ShowVesselsInThePast) return;

            if (Enabled)
            {
                var vesselsToUnload = SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels
                                       .Where(v => !v.Value.NeedsToBeReloaded && VesselCommon.VesselIsControlledAndInPastSubspace(v.Key))
                                       .Select(v => FlightGlobals.FindVessel(v.Key))
                                       .ToArray();

                if (vesselsToUnload.Any())
                {
                    LunaLog.Log($"[LMP]: Unloading {vesselsToUnload.Length} vessels that are in a past subspace");
                    foreach (var vessel in vesselsToUnload)
                    {
                        AddToKillList(vessel);
                    }
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Kills or unloads a vessel.
        /// If you set fullKill to true the vessel will be totally removed from the game, 
        /// otherwise is is killed but can be re-created at a later time (once you are in the same subspace for example)
        /// </summary>
        private static void KillVessel(Vessel killVessel)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel) || killVessel.state == Vessel.State.DEAD)
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
                var otherVessels = FlightGlobals.Vessels.Where(v => v.id != killVessel.id).ToArray();

                if (otherVessels.Any())
                    FlightGlobals.ForceSetActiveVessel(otherVessels.First());
                else
                    HighLogic.LoadScene(GameScenes.SPACECENTER);

                ScreenMessages.PostScreenMessage("The vessel you were spectating was removed", 3f);
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

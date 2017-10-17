using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionAltSys;
using LunaClient.Systems.VesselProtoSys;
using System;
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

        #endregion

        #region Base overrides

        protected override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Add(VesselRemoveEvents.OnVesselDestroyed);
            SetupRoutine(new RoutineDefinition(SettingsSystem.ServerSettings.VesselKillCheckMsInterval,
                RoutineExecution.Update, KillPastSubspaceVessels));
        }

        protected override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Remove(VesselRemoveEvents.OnVesselDestroyed);
        }

        #endregion

        #region Public

        /// <summary>
        /// Unloads (but not fully kill) the given vessels
        /// </summary>
        /// <param name="killVessel"></param>
        public void UnloadVessels(Vessel[] killVessel)
        {
            foreach (var vessel in killVessel)
            {
                KillVessel(vessel, false);
            }
        }

        /// <summary>
        /// Kills or unloads a vessel.
        /// If you set fullKill to true the vessel will be totally removed from the game, 
        /// otherwise is is killed but can be re-created at a later time (once you are in the same subspace for example)
        /// </summary>
        public void KillVessel(Vessel killVessel, bool fullKill)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel) || killVessel.state == Vessel.State.DEAD)
                return;

            LunaLog.Log($"[LMP]: Killing vessel {killVessel.id}");

            SystemsContainer.Get<VesselPositionAltSystem>().RemoveVessel(killVessel);
            if (SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels.ContainsKey(killVessel.id))
            {
                if (!fullKill)
                    SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels[killVessel.id].Loaded = false;
                else
                {
                    SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels.TryRemove(killVessel.id, out var _);
                }
            }

            SwitchVesselIfSpectating(killVessel);
            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);
        }

        #endregion

        #region Update methods

        /// <summary>
        /// Get the vessels that are in a past subspace and kill them
        /// </summary>
        private void KillPastSubspaceVessels()
        {
            if (SettingsSystem.ServerSettings.ShowVesselsInThePast) return;

            if (Enabled && SystemsContainer.Get<MainSystem>().GameRunning)
            {
                var vesselsToUnload = SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels
                                       .Where(v => v.Value.Loaded && VesselCommon.VesselIsControlledAndInPastSubspace(v.Key))
                                       .Select(v => FlightGlobals.FindVessel(v.Key))
                                       .ToArray();

                if (vesselsToUnload.Any())
                {
                    LunaLog.Log($"[LMP]: Unloading {vesselsToUnload.Length} vessels that are in a past subspace");
                    UnloadVessels(vesselsToUnload);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Unloads a vessel from the game in 1 frame.
        /// </summary>
        public void UnloadVessel(Vessel killVessel)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel) || killVessel.state == Vessel.State.DEAD)
            {
                return;
            }

            if (SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels.ContainsKey(killVessel.id))
            {
                SystemsContainer.Get<VesselProtoSystem>().AllPlayerVessels[killVessel.id].Loaded = false;
            }

            //TODO: Should we put the vessel on rails so that we don't run into bugs destroying the vessel?
            //killVessel.GoOnRails();

            FlightGlobals.fetch.SetVesselTarget(null, true);
            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);

            //TODO: Do we need to do this?  DMP has this code
            //Add it to the delay kill list to make sure it dies.
            //With KSP, If it doesn't work first time, keeping doing it until it does.
            //if (!delayKillVessels.Contains(killVessel))
            //{
            //   delayKillVessels.Add(killVessel);
            //}
        }

        private static void SwitchVesselIfSpectating(Vessel killVessel)
        {
            if (VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id == killVessel.id)
            {
                var otherVessels = FlightGlobals.Vessels.Where(v => v.id != killVessel.id).ToArray();

                if (otherVessels.Any())
                    FlightGlobals.ForceSetActiveVessel(otherVessels.First());
                else
                    HighLogic.LoadScene(GameScenes.SPACECENTER);

                ScreenMessages.PostScreenMessage("The player you were spectating removed his vessel");
            }
        }

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

using System;
using System.Collections;
using LunaClient.Base;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using UniLinq;
using UnityEngine;

namespace LunaClient.Systems.VesselRemoveSys
{
    /// <summary>
    /// This system handles the killing of vessels. We kill the vessels that are not in our subspace and 
    /// the vessels that are destroyed, old copies of changed vessels or when they dock
    /// </summary>
    public class VesselRemoveSystem : MessageSystem<VesselRemoveSystem, VesselRemoveMessageSender, VesselRemoveMessageHandler>
    {
        #region Fields

        private VesselRemoveEvents VesselRemoveEvents { get; } = new VesselRemoveEvents();

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Add(VesselRemoveEvents.OnVesselDestroyed);
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Remove(VesselRemoveEvents.OnVesselDestroyed);
        }

        /// <summary>
        /// Get the vessels that are in a past subspace and kill them
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (SettingsSystem.ServerSettings.ShowVesselsInThePast) return;

            if (Enabled && MainSystem.Singleton.GameRunning && 
                Timer.ElapsedMilliseconds > SettingsSystem.ServerSettings.VesselKillCheckMsInterval)
            {
                var vesselsToUnload = VesselProtoSystem.Singleton.AllPlayerVessels
                                       .Where(v => v.Value.Loaded && VesselCommon.VesselIsControlledAndInPastSubspace(v.Key))
                                       .Select(v => FlightGlobals.FindVessel(v.Key))
                                       .ToArray();

                if (vesselsToUnload.Any())
                {
                    Debug.Log($"[LMP]: Unloading {vesselsToUnload.Length} vessels that are in a past subspace");
                    UnloadVessels(vesselsToUnload);
                }
                ResetTimer();
            }
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

            Debug.Log($"[LMP]: Killing vessel {killVessel.id}");
            
            if (VesselProtoSystem.Singleton.AllPlayerVessels.ContainsKey(killVessel.id))
            {
                if (!fullKill)
                    VesselProtoSystem.Singleton.AllPlayerVessels[killVessel.id].Loaded = false;
                else
                {
                    VesselProtoUpdate val;
                    VesselProtoSystem.Singleton.AllPlayerVessels.TryRemove(killVessel.id, out val);
                }
            }

            SwitchVesselIfSpectating(killVessel);
            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);
        }

        #endregion

        #region Private methods
        
        /// <summary>
        /// Unloads a vessel from the game in 1 frame.
        /// </summary>
        public void UnloadVessel(Vessel killVessel)
        {
            if (killVessel == null || !FlightGlobals.Vessels.Contains(killVessel) || killVessel.state == Vessel.State.DEAD)
                return;
            
            if (VesselProtoSystem.Singleton.AllPlayerVessels.ContainsKey(killVessel.id))
            {
                VesselProtoSystem.Singleton.AllPlayerVessels[killVessel.id].Loaded = false;
            }

            UnloadVesselFromGame(killVessel);
            KillGivenVessel(killVessel);
            UnloadVesselFromScenario(killVessel);
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
                Debug.LogError("[LMP]: Error destroying vessel from the scenario: " + destroyException);
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
                Debug.LogError("[LMP]: Error destroying vessel: " + killException);
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
                    Debug.LogError($"[LMP]: Error unloading vessel: {unloadException}");
                }
            }
        }

        #endregion
    }
}

using System;
using LunaClient.Base;
using LunaClient.Systems.AtmoLoader;
using LunaClient.Systems.PartKiller;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.TimeSyncer;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselUpdateSys;
using LunaClient.Systems.VesselWarpSys;
using LunaClient.Systems.Warp;
using LunaClient.Utilities;
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

        private bool _enabled;

        public override bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (!_enabled && value)
                    RegisterGameHooks();
                else if (_enabled && !value)
                    UnregisterGameHooks();

                _enabled = value;
            }
        }

        private long LastVesselKillCheck { get; set; }

        private VesselRemoveEvents VesselRemoveEvents { get; } = new VesselRemoveEvents();

        #endregion

        #region Base overrides

        /// <summary>
        /// Check the vessels that are not in our subspace and kill them
        /// </summary>
        public override void FixedUpdate()
        {
            if (DateTime.UtcNow.Ticks - LastVesselKillCheck > TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselKillCheckMsInterval).Ticks)
            {
                LastVesselKillCheck = DateTime.UtcNow.Ticks;

                var vesselsToKill = VesselProtoSystem.Singleton.AllPlayerVessels
                    .Where(v => v.Loaded && VesselWarpSystem.Singleton.GetVesselSubspace(v.VesselId) != WarpSystem.Singleton.CurrentSubspace)
                    .ToList();

                KillVessels(vesselsToKill.Select(v => FlightGlobals.FindVessel(v.VesselId)).ToArray());

                foreach (var killedVessel in vesselsToKill)
                {
                    killedVessel.Loaded = false;
                }
            }
        }

        #endregion

        #region Public

        public void KillVessels(Vessel[] killVessel)
        {
            foreach (var vessel in killVessel)
            {
                KillVessel(vessel);
            }
        }

        public void KillVessel(Vessel killVessel)
        {
            if (!FlightGlobals.Vessels.Contains(killVessel)) return;

            while (true)
            {
                if (killVessel != null)
                {
                    LunaLog.Debug("Killing vessel: " + killVessel.id);

                    //Forget the dying vessel
                    PartKillerSystem.Singleton.ForgetVessel(killVessel);
                    AtmoLoaderSystem.Singleton.ForgetVessel(killVessel);

                    //Try to unload the vessel first.
                    if (killVessel.loaded)
                    {
                        try
                        {
                            killVessel.Unload();
                        }
                        catch (Exception unloadException)
                        {
                            LunaLog.Debug("Error unloading vessel: " + unloadException);
                        }
                    }

                    //Remove the kerbal from the craft
                    foreach (var pps in killVessel.protoVessel.protoPartSnapshots)
                        foreach (var pcm in pps.protoModuleCrew.ToArray())
                            pps.RemoveCrew(pcm);

                    try
                    {
                        killVessel.Die();
                    }
                    catch (Exception killException)
                    {
                        LunaLog.Debug("Error destroying vessel: " + killException);
                    }

                    try
                    {
                        HighLogic.CurrentGame.DestroyVessel(killVessel);
                    }
                    catch (Exception destroyException)
                    {
                        LunaLog.Debug("Error destroying vessel from the scenario: " + destroyException);
                    }

                    if (FlightGlobals.Vessels.Contains(killVessel) && (killVessel.state != Vessel.State.DEAD))
                    {
                        continue;//Recursive Killing
                    }
                }
                break;
            }
        }

        #endregion

        #region Private methods

        private void RegisterGameHooks()
        {
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Add(VesselRemoveEvents.OnVesselDestroyed);
        }

        private void UnregisterGameHooks()
        {
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Remove(VesselRemoveEvents.OnVesselDestroyed);
        }

        #endregion
    }
}

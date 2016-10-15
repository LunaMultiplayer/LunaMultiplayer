using System;
using System.Collections;
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
        
        private VesselRemoveEvents VesselRemoveEvents { get; } = new VesselRemoveEvents();

        #endregion

        #region Base overrides

        public override void OnEnabled()
        {
            base.OnEnabled();
            GameEvents.onVesselRecovered.Add(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Add(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Add(VesselRemoveEvents.OnVesselDestroyed);
            Client.Singleton.StartCoroutine(CheckVesselsToKill());
        }

        public override void OnDisabled()
        {
            base.OnDisabled();
            GameEvents.onVesselRecovered.Remove(VesselRemoveEvents.OnVesselRecovered);
            GameEvents.onVesselTerminated.Remove(VesselRemoveEvents.OnVesselTerminated);
            GameEvents.onVesselDestroy.Remove(VesselRemoveEvents.OnVesselDestroyed);
        }
        
        #endregion

        /// <summary>
        /// Check the vessels that are not in our subspace and kill them
        /// </summary>
        private IEnumerator CheckVesselsToKill()
        {
            var seconds = new WaitForSeconds((float)TimeSpan.FromMilliseconds(SettingsSystem.ServerSettings.VesselKillCheckMsInterval).TotalSeconds);
            while (true)
            {
                if (!Enabled) break;

                var vesselsToKill = VesselProtoSystem.Singleton.AllPlayerVessels
                    .Where(v => v.Loaded && VesselWarpSystem.Singleton.GetVesselSubspace(v.VesselId) != WarpSystem.Singleton.CurrentSubspace)
                    .ToList();

                KillVessels(vesselsToKill.Select(v => FlightGlobals.FindVessel(v.VesselId)).ToArray());

                foreach (var killedVessel in vesselsToKill)
                {
                    killedVessel.Loaded = false;
                }

                yield return seconds;
            }
        }

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
            Client.Singleton.StartCoroutine(KillVesselRoutine(killVessel));
        }

        private static IEnumerator KillVesselRoutine(Vessel killVessel)
        {
            while (true)
            {
                if (!FlightGlobals.Vessels.Contains(killVessel)) break;

                if (killVessel != null)
                {
                    Debug.Log("Killing vessel: " + killVessel.id);

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
                            Debug.Log("Error unloading vessel: " + unloadException);
                        }
                    }

                    yield return null; //Resume on next frame

                    //Remove the kerbal from the craft
                    foreach (var pps in killVessel.protoVessel.protoPartSnapshots)
                        foreach (var pcm in pps.protoModuleCrew.ToArray())
                            pps.RemoveCrew(pcm);

                    yield return null; //Resume on next frame

                    try
                    {
                        killVessel.Die();
                    }
                    catch (Exception killException)
                    {
                        Debug.Log("Error destroying vessel: " + killException);
                    }

                    yield return null; //Resume on next frame

                    //try
                    //{
                    //    HighLogic.CurrentGame.DestroyVessel(killVessel);
                    //    HighLogic.CurrentGame.Updated();
                    //}
                    //catch (Exception destroyException)
                    //{
                    //    Debug.Log("Error destroying vessel from the scenario: " + destroyException);
                    //}

                    //yield return null; //Resume on next frame

                    if (FlightGlobals.Vessels.Contains(killVessel) && (killVessel.state != Vessel.State.DEAD))
                    {
                        continue; //Recursive Killing
                    }
                }
                break;
            }
        }

        #endregion
    }
}

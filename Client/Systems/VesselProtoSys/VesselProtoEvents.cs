using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using System;
using System.Linq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents : SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (!VesselCommon.IsSpectating && FlightGlobals.ActiveVessel != null)
            {
                CoroutineUtil.StartDelayedRoutine(nameof(FlightReady), () =>
                {
                    if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                        return;

                    System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
                }, 5f);

                ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.SafetyBubble, 10f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        /// <summary>
        /// Called when a vessel is initiated.
        /// </summary>
        public void VesselCreate(Vessel data)
        {
            //We are just reloading a vessel and the vessel.Load() was triggered so we should not do anything!
            if (data.id == VesselLoader.ReloadingVesselId)
                return;

            //No need to check the unloaded update locks as vessels when unloaded don't have parts!
            if (!VesselCommon.IsSpectating && data.id != VesselProtoSystem.CurrentlyUpdatingVesselId && !VesselRemoveSystem.Singleton.VesselWillBeKilled(data.id))
            {
                //We are modifying a vessel that LMP is not handling
                if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.id))
                {
                    //The vessel even exists on the store so probably it's a vessel that has lost a part or smth like that...
                    if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(data.id, SettingsSystem.CurrentSettings.PlayerName))
                    {
                        //We own the update lock of that vessel that suffered a modification so just leave it here
                        //The main system has a routine that will check changes and send the new definition
                        LunaLog.Log($"SKIPPING detected change in vesselId {data.id} name {data.vesselName} (we own update lock)");
                    }
                    else
                    {
                        LunaLog.Log($"REVERTING change in NEW vesselId {data.id} name {data.vesselName} (DON'T own UnloadedUpdate lock)");
                        VesselsProtoStore.AllPlayerVessels[data.id].VesselHasUpdate = true;
                    }
                }
                else
                {
                    //The vessel is NEW as it's not in the store. It might be a debris...
                    var rootPartOrFirstPart = data.rootPart ?? data.parts.FirstOrDefault();
                    if (rootPartOrFirstPart != null)
                    {
                        var originalVessel = VesselsProtoStore.GetVesselByPartId(rootPartOrFirstPart.flightID);
                        if (originalVessel == null)
                        {
                            //We didn't find an original vessel so it's probably a totally new vessel that was spawned...
                            return;
                        }

                        //The vessel even exists on the store so probably it's a vessel that has lost a part or smth like that...
                        if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                        {
                            LunaLog.Log($"SENDING NEW vesselId {data.id} name {data.vesselName} (Original vessel UPD lock is ours)");

                            //We own the update lock of that vessel that originated that part so let's get that update lock as forced 
                            //and send the definition with the main system routine
                            LockSystem.Singleton.AcquireUpdateLock(data.id, true);
                        }
                        else
                        {
                            LunaLog.Log($"REVERTING NEW vesselId {data.id} name {data.vesselName} (UPD lock is NOT ours)");
                            VesselRemoveSystem.Singleton.AddToKillList(data.id);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Event called when switching scene and before reaching the other scene
        /// </summary>
        internal void OnSceneRequested(GameScenes requestedScene)
        {
            if (HighLogic.LoadedSceneIsFlight && requestedScene != GameScenes.FLIGHT)
            {
                //When quitting flight send the vessel one last time
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
            }
        }
        
        /// <summary>
        /// Event called when a part is dead and removed from the game
        /// </summary>
        public void OnPartDie(Part data)
        {
            if (VesselCommon.IsSpectating || data.vessel == null) return;
            if (data.vessel.id != VesselProtoSystem.CurrentlyUpdatingVesselId && !VesselRemoveSystem.Singleton.VesselWillBeKilled(data.vessel.id))
            {
                if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.vessel.id))
                {
                    if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(data.vessel.id, SettingsSystem.CurrentSettings.PlayerName))
                    {
                        VesselsProtoStore.AllPlayerVessels[data.vessel.id].VesselHasUpdate = true;
                    }
                }
                else
                {
                    //The vessel is NEW as it's not in the store. It might be a debris...
                    var rootPartOrFirstPart = data.vessel.rootPart ?? data.vessel.parts.FirstOrDefault();
                    if (rootPartOrFirstPart != null)
                    {
                        var originalVessel = VesselsProtoStore.GetVesselByPartId(rootPartOrFirstPart.flightID);
                        if (originalVessel == null)
                        {
                            return;
                        }

                        if (!LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                        {
                            VesselsProtoStore.AllPlayerVessels[originalVessel.id].VesselHasUpdate = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Triggered when the vessel parts change. We use this to detect if we are spectating and our vessel is different than the controller. 
        /// If that's the case we trigger a reload
        /// </summary>
        public void VesselPartCountChanged(Vessel vessel)
        {
            if (!VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || (VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id != vessel.id)) return;

            if (VesselLoader.ReloadingVesselId == vessel.id) return;

            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(FlightGlobals.ActiveVessel.id, out var vesselProtoUpdate))
            {
                if (vesselProtoUpdate.ProtoVessel.protoPartSnapshots.Count != FlightGlobals.ActiveVessel.Parts.Count)
                    vesselProtoUpdate.VesselHasUpdate = true;
            }
        }
    }
}

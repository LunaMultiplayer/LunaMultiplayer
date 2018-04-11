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
                if (!System.CheckVessel(FlightGlobals.ActiveVessel))
                {
                    VesselRemoveSystem.Singleton.AddToKillList(FlightGlobals.ActiveVessel.id);
                    VesselRemoveSystem.Singleton.KillVessel(FlightGlobals.ActiveVessel.id);
                    return;
                }

                CoroutineUtil.StartDelayedRoutine(nameof(FlightReady), () =>
                {
                    if (VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || FlightGlobals.ActiveVessel.id == Guid.Empty)
                        return;
                    
                    System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
                }, 5f);

                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.SafetyBubble, 10f, ScreenMessageStyle.UPPER_CENTER);
            }
        }

        /// <summary>
        /// Called when a vessel is initiated.
        /// </summary>
        public void VesselCreate(Vessel data)
        {
            //We just created a vessel that exists in the store so we can just ignore this
            if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.id))
                return;
            
            //The vessel is NEW as it's not in the store. It might be a debris...
            var rootPartOrFirstPartFlightId = data.rootPart?.flightID ?? data.parts.FirstOrDefault()?.flightID ?? data.protoVessel?.protoPartSnapshots?.FirstOrDefault()?.flightID ?? 0;
            if (rootPartOrFirstPartFlightId != 0)
            {
                var originalVessel = VesselsProtoStore.GetVesselByPartId(rootPartOrFirstPartFlightId);
                if (originalVessel == null)
                {
                    //We didn't find an original vessel so it's probably a totally new vessel that spawned...
                    LunaLog.Log($"SENDING NEW vesselId {data.id} name {data.vesselName} (Original vessel NOT found)");

                    System.MessageSender.SendVesselMessage(data, true);
                    LockSystem.Singleton.AcquireUpdateLock(data.id, true);
                }
                else
                {
                    //The vessel even exists on the store so probably it's a vessel that has lost a part or smth like that...
                    if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                    {
                        //We own the update lock of that vessel that originated that part so let's get that update lock as forced and send the definition
                        LunaLog.Log($"SENDING NEW vesselId {data.id} name {data.vesselName} (Original vessel UPD lock is ours)");

                        System.MessageSender.SendVesselMessage(data, true);
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
        public void VesselPartCountChangedinSpectatingVessel(Vessel vessel)
        {
            if (vessel == null) return;

            if (!VesselCommon.IsSpectating || FlightGlobals.ActiveVessel == null || (VesselCommon.IsSpectating && FlightGlobals.ActiveVessel.id != vessel.id)) return;

            if (VesselLoader.ReloadingVesselId == vessel.id) return;

            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(FlightGlobals.ActiveVessel.id, out var vesselProtoUpdate))
            {
                if (vesselProtoUpdate.ProtoVessel.protoPartSnapshots.Count != FlightGlobals.ActiveVessel.Parts.Count)
                    vesselProtoUpdate.VesselHasUpdate = true;
            }
        }

        /// <summary>
        /// Triggered when the vessel parts change. We use this to detect if somehow a vessel of another player has lost a part
        /// </summary>
        public void VesselPartCountChanged(Vessel vessel)
        {
            if (vessel == null) return;

            if (VesselLoader.ReloadingVesselId == vessel.id || vessel.id == FlightGlobals.ActiveVessel?.id) return;
            if (LockSystem.LockQuery.UpdateLockBelongsToPlayer(vessel.id, SettingsSystem.CurrentSettings.PlayerName)) return;

            if (VesselsProtoStore.AllPlayerVessels.TryGetValue(vessel.id, out var vesselProtoUpdate))
            {
                if (vesselProtoUpdate.ProtoVessel.protoPartSnapshots.Count > vessel.Parts.Count)
                    vesselProtoUpdate.VesselHasUpdate = true;
            }
        }
    }
}

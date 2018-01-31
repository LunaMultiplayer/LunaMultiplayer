using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.VesselStore;
using System;
using System.Linq;

namespace LunaClient.Systems.VesselProtoSys
{
    public class VesselProtoEvents: SubSystem<VesselProtoSystem>
    {
        /// <summary>
        /// Sends our vessel just when we start the flight
        /// </summary>
        public void FlightReady()
        {
            if (FlightGlobals.ActiveVessel != null)
            {
                System.MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel, true);
                //Add our own vessel to the dictionary aswell
                VesselsProtoStore.AddVesselToDictionary(FlightGlobals.ActiveVessel);
            }
        }

        /// <summary>
        /// Called when a vessel is initiated.
        /// </summary>
        public void VesselCreate(Vessel data)
        {
            //No need to check the unloaded update locks as vessels when unloaded don't have parts!

            if (data.id != VesselProtoSystem.CurrentlyUpdatingVesselId && !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(data.id))
            {
                //We are modifying a vessel that LMP is not handling
                if (VesselsProtoStore.AllPlayerVessels.ContainsKey(data.id))
                {
                    //The vessel even exists on the store so probably it's a vessel that has lost a part or smth like that...
                    if(LockSystem.LockQuery.UpdateLockBelongsToPlayer(data.id, SettingsSystem.CurrentSettings.PlayerName))
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

                            //We own the update lock of that vessel that originated that part so let's get that updat lock and send the definition
                            SystemsContainer.Get<LockSystem>().AcquireUpdateLock(data.id, true);
                            //Now send this debris and force it!
                            System.MessageSender.SendVesselMessage(data, true);
                            //Add it also to our store
                            VesselsProtoStore.AddVesselToDictionary(FlightGlobals.ActiveVessel);
                        }
                        else
                        {
                            LunaLog.Log($"REVERTING NEW vesselId {data.id} name {data.vesselName} (UPD lock is NOT ours)");
                            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(data.id);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// We use this method to detect when a flag has been planted and we are far away from it.
        /// We don't use the onflagplanted event as that is triggered too early and we need to set the id
        /// AFTER we filled the plaque in the flag
        /// </summary>
        /// <param name="data"></param>
        public void VesselGoOnRails(Vessel data)
        {
            if (data.vesselType == VesselType.Flag && data.id == Guid.Empty)
            {
                data.id = Guid.NewGuid();
                data.protoVessel.vesselID = data.id;
                System.MessageSender.SendVesselMessage(data, true);
            }
        }

        /// <summary>
        /// Event called when a part is dead and removed from the game
        /// </summary>
        public void OnPartDie(Part data)
        {
            if(data.vessel == null) return;
            if (data.vessel.id != VesselProtoSystem.CurrentlyUpdatingVesselId && !SystemsContainer.Get<VesselRemoveSystem>().VesselWillBeKilled(data.vessel.id))
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
    }
}

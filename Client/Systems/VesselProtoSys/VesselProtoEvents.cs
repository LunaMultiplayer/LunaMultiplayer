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
                        return;
                    }
                    else
                    {
                        if (!LockSystem.LockQuery.UpdateLockExists(data.id))
                        {
                            if(LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(data.id, SettingsSystem.CurrentSettings.PlayerName))
                            {
                                //Vessel is probably very far away and nobody is near it. Still, as we have the unloaded update lock we must
                                //update the vessel definition. We don't need to force it anyway
                                LunaLog.Log($"SENDING change in NEW vesselId {data.id} name {data.vesselName} (we own UnloadedUpdate lock)");
                                System.MessageSender.SendVesselMessage(data, false);
                            }
                            else
                            {
                                //Nobody has the unloaded update/update lock or perhaps another person has it.
                                //Therefore roll back the changes we detected
                                VesselsProtoStore.AllPlayerVessels[data.id].VesselHasUpdate = true;
                                LunaLog.Log($"REVERTING change in NEW vesselId {data.id} name {data.vesselName} (DON'T own UnloadedUpdate lock)");
                            }
                        }
                        else
                        {
                            //Somebody else has the update lock, so roll back the changes we detected
                            LunaLog.Log($"REVERTING change in NEW vesselId {data.id} name {data.vesselName} (DON'T own UnloadedUpdate lock)");
                            VesselsProtoStore.AllPlayerVessels[data.id].VesselHasUpdate = true;
                        }
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
                            return;
                        }
                        else
                        {
                            if (!LockSystem.LockQuery.UpdateLockExists(originalVessel.id))
                            {
                                if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(originalVessel.id, SettingsSystem.CurrentSettings.PlayerName))
                                {
                                    //Original vessel is probably very far away and nobody is near it. Still, as we have the unloaded update lock we must
                                    //update the vessel debris definition.
                                    LunaLog.Log($"SENDING NEW vesselId {data.id} name {data.vesselName} (Original vessel UNL UPD lock is ours)");
                                    //We own the update lock of that vessel that originated that part so let's get that updat lock and send the definition
                                    SystemsContainer.Get<LockSystem>().AcquireUpdateLock(data.id, true);
                                    //Now send this debris and force it!
                                    System.MessageSender.SendVesselMessage(data, true);
                                    //Add it also to our store
                                    VesselsProtoStore.AddVesselToDictionary(FlightGlobals.ActiveVessel);
                                }
                                else
                                {
                                    LunaLog.Log($"REVERTING NEW vesselId {data.id} name {data.vesselName} (UNL UPD lock is NOT ours)");

                                    //Nobody has the unloaded update/update lock or perhaps another person has it.
                                    //Therefore roll back the changes we detected
                                    SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(data.id);
                                }
                            }
                            else
                            {
                                LunaLog.Log($"REVERTING NEW vesselId {data.id} name {data.vesselName} (UPD lock is NOT ours)");

                                //Somebody else has the update lock, so roll back the changes we detected
                                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(data.id);
                            }
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
    }
}

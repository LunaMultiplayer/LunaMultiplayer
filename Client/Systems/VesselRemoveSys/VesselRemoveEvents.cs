using LunaClient.Base;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Utilities;
using LunaClient.VesselUtilities;
using System;
using UniLinq;

namespace LunaClient.Systems.VesselRemoveSys
{
    public class VesselRemoveEvents : SubSystem<VesselRemoveSystem>
    {
        private static Guid _recoveringTerminatingVesselId = Guid.Empty;

        /// <summary>
        /// This event is called when the vessel gone BOOM (the Vessel.Die() is called)
        /// If we have the update lock of it we kill it
        /// It doesn't matter if we own the control lock or not as perhaps we are killing a vessel of a player who disconnected.
        /// </summary>
        public void OnVesselWillDestroy(Vessel dyingVessel)
        {
            //We are just reloading a vessel and the vessel.Die() was triggered so we should not do anything!
            if (dyingVessel.id == VesselLoader.ReloadingVesselId)
                return;

            //Only send the vessel remove msg if we own the unloaded update lock
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(dyingVessel.id, SettingsSystem.CurrentSettings.PlayerName) || dyingVessel.id == _recoveringTerminatingVesselId)
            {
                var reason = dyingVessel.id == _recoveringTerminatingVesselId ? "Recovered/Terminated" : "Destroyed";
                LunaLog.Log($"[LMP]: Removing vessel {dyingVessel.id}, Name: {dyingVessel.vesselName} from the server: {reason}");

                //Add to the kill list so it's also removed from the store later on!
                System.AddToKillList(dyingVessel.id);

                SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(dyingVessel);

                var killingOwnVessel = FlightGlobals.ActiveVessel?.id == dyingVessel.id;

                //If we are killing our own vessel there's the possibility that the player hits "revert" so in this case
                //DO NOT keep it in the remove list
                System.MessageSender.SendVesselRemove(dyingVessel.id, !killingOwnVessel);

                //Vessel is dead so remove the locks after 1500ms to get the debris locks if any
                SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(dyingVessel.id, 1500);
            }
        }

        /// <summary>
        /// This event is called when the vessel is recovered
        /// </summary>
        public void OnVesselRecovered(ProtoVessel recoveredVessel, bool quick)
        {
            //quick == true when you press "space center" from the inflight menu

            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(recoveredVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                ScreenMessages.PostScreenMessage("Cannot recover vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = recoveredVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {recoveredVessel.vesselID}, Name: {recoveredVessel.vesselName} from the server: Recovered");
            SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(recoveredVessel);

            System.MessageSender.SendVesselRemove(recoveredVessel.vesselID);

            //Vessel is recovered so remove the locks
            SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(recoveredVessel.vesselID);
        }

        /// <summary>
        /// This event is called when vessel is terminated from track station
        /// </summary>
        public void OnVesselTerminated(ProtoVessel terminatedVessel)
        {
            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(terminatedVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                ScreenMessages.PostScreenMessage("Cannot terminate vessel, the vessel is not yours.", 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = terminatedVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {terminatedVessel.vesselID}, Name: {terminatedVessel.vesselName} from the server: Terminated");
            SystemsContainer.Get<KerbalSystem>().ProcessKerbalsInVessel(terminatedVessel);

            System.MessageSender.SendVesselRemove(terminatedVessel.vesselID);

            //Vessel is terminated so remove locks            
            SystemsContainer.Get<LockSystem>().ReleaseAllVesselLocks(terminatedVessel.vesselID);
        }

        /// <summary>
        /// This event is called after a game is loaded. We use it to detect if the player has done a revert
        /// </summary>
        public void OnGameStatePostLoad(ConfigNode data)
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                LunaLog.Log("[LMP]: Detected a revert!");
                var vesselIdsToRemove = FlightGlobals.Vessels
                    .Where(v => v.rootPart?.missionID == FlightGlobals.ActiveVessel.rootPart.missionID && v.id != FlightGlobals.ActiveVessel.id)
                    .Select(v => v.id).Distinct();

                //We detected a revert, now pick all the vessel parts (debris) that came from our main active 
                //vessel and remove them both from our game and server
                foreach (var vesselIdToRemove in vesselIdsToRemove)
                {
                    System.MessageSender.SendVesselRemove(vesselIdToRemove);
                    System.AddToKillList(vesselIdToRemove);
                }

                //Store it here so the delayed routine can access it!
                var activeVesselId = FlightGlobals.ActiveVessel.id;

                //Now tell the server to remove our old vessel
                CoroutineUtil.StartDelayedRoutine("SendProperVesselRemoveMsg", () =>
                {
                    //We delay the send vessel remove to wait until the proper scene is loaded.
                    //In case we revert to editor we must fully delete that vessel as when flying again we will get a new ID.
                    //Otherwise we say to not keep it in the vessels remove list as perhaps we are reverting to flight and then our vessel id will stay the same. 
                    //If we set the keepvesselinremovelist to true then the server will ignore every change we do to our vessel! 
                    System.MessageSender.SendVesselRemove(activeVesselId, HighLogic.LoadedSceneIsEditor);
                }, 3);
            }
        }
    }
}
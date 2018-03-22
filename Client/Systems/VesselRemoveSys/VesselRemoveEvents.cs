using LunaClient.Base;
using LunaClient.Localization;
using LunaClient.Systems.KerbalSys;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
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
            if (VesselLoader.ReloadingVesselId == dyingVessel.id)
                return;

            //We are MANUALLY killing a vessel and it's NOT KSP who is calling this mehtod so ignore all the logic of below
            if (System.ManuallyKillingVesselId == dyingVessel.id)
                return;

            //Only send the vessel remove msg if we own the unloaded update lock
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(dyingVessel.id, SettingsSystem.CurrentSettings.PlayerName) || dyingVessel.id == _recoveringTerminatingVesselId)
            {
                var reason = dyingVessel.id == _recoveringTerminatingVesselId ? "Recovered/Terminated" : "Destroyed";
                LunaLog.Log($"[LMP]: Removing vessel {dyingVessel.id}, Name: {dyingVessel.vesselName} from the server: {reason}");

                //Add to the kill list so it's also removed from the store later on!
                System.AddToKillList(dyingVessel.id);

                KerbalSystem.Singleton.ProcessKerbalsInVessel(dyingVessel);

                var killingOwnVessel = FlightGlobals.ActiveVessel?.id == dyingVessel.id;

                //If we are killing our own vessel there's the possibility that the player hits "revert" so in this case
                //DO NOT keep it in the remove list
                System.MessageSender.SendVesselRemove(dyingVessel.id, !killingOwnVessel);

                //Vessel is dead so remove the locks after 1500ms to get the debris locks if any
                LockSystem.Singleton.ReleaseAllVesselLocks(dyingVessel.id, 1500);
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
                ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.CannotRecover, 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = recoveredVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {recoveredVessel.vesselID}, Name: {recoveredVessel.vesselName} from the server: Recovered");
            KerbalSystem.Singleton.ProcessKerbalsInVessel(recoveredVessel);

            System.MessageSender.SendVesselRemove(recoveredVessel.vesselID);

            //Vessel is recovered so remove the locks
            LockSystem.Singleton.ReleaseAllVesselLocks(recoveredVessel.vesselID);
        }

        /// <summary>
        /// This event is called when vessel is terminated from track station
        /// </summary>
        public void OnVesselTerminated(ProtoVessel terminatedVessel)
        {
            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(terminatedVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                ScreenMessages.PostScreenMessage(LocalizationContainer.ScreenText.CannotTerminate, 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = terminatedVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {terminatedVessel.vesselID}, Name: {terminatedVessel.vesselName} from the server: Terminated");
            KerbalSystem.Singleton.ProcessKerbalsInVessel(terminatedVessel);

            System.MessageSender.SendVesselRemove(terminatedVessel.vesselID);

            //Vessel is terminated so remove locks            
            LockSystem.Singleton.ReleaseAllVesselLocks(terminatedVessel.vesselID);
        }

        /// <summary>
        /// Triggered when reverting back to the launchpad. The vessel id does NOT change
        /// </summary>
        public void OnRevertToLaunch()
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                LunaLog.Log("[LMP]: Detected a revert to launch!");
                RemoveOldVesselAndItsDebris(FlightGlobals.ActiveVessel);
                System.MessageSender.SendVesselRemove(FlightGlobals.ActiveVessel.id, false);
            }
        }

        /// <summary>
        /// Triggered when reverting back to the editor. The vessel id DOES change
        /// </summary>
        public void OnRevertToEditor(EditorFacility data)
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                LunaLog.Log($"[LMP]: Detected a revert to editor! {data}");
                System.AddToKillList(FlightGlobals.ActiveVessel.id);
                RemoveOldVesselAndItsDebris(FlightGlobals.ActiveVessel);
                System.MessageSender.SendVesselRemove(FlightGlobals.ActiveVessel.id, true);
            }
        }

        private static void RemoveOldVesselAndItsDebris(Vessel vessel)
        {            
            //We detected a revert, now pick all the vessel parts (debris) that came from our main active 
            //vessel and remove them both from our game and server
            var vesselIdsToRemove = FlightGlobals.Vessels
                .Where(v => v.rootPart?.missionID == vessel.rootPart.missionID && v.id != vessel.id)
                .Select(v => v.id).Distinct();
            
            foreach (var vesselIdToRemove in vesselIdsToRemove)
            {
                System.MessageSender.SendVesselRemove(vesselIdToRemove);
                System.AddToKillList(vesselIdToRemove);
            }
        }

        /// <summary>
        /// This method is called just when the game scene is loaded. 
        /// We use this to detect when switching to flight and then remove the vessels that are inside the safety bubble
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            if (data == GameScenes.FLIGHT)
                System.RemoveVesselsInSafetyBubble();
        }
    }
}

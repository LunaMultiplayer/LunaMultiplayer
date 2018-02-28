using LunaClient.Base;
using LunaClient.Localization;
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
                    if (HighLogic.LoadedSceneIsEditor) System.AddToKillList(activeVesselId);
                }, 3);
            }
        }

        /// <summary>
        /// Triggered when requesting a scene change. If we are leaving flight send our protovessel one last time
        /// </summary>
        public void OnSceneRequested(GameScenes requestedScene)
        {
            if (requestedScene == GameScenes.FLIGHT) return;

            DelayedClearVessels();
        }

        /// <summary>
        /// Called when the scene changes
        /// </summary>
        public void OnSceneChanged(GameScenes data)
        {
            if (data == GameScenes.SPACECENTER)
            {
                //If we are going to space center clear all the vessels.
                //This will avoid all the headaches of recovering vessels and so on with the KSCVesselMarkers.
                //Those markers appear on the KSC when you return from flight but they are NEVER updated
                //So if a vessel from another player was in the launchpad and you return to the KSC, even 
                //if that player goes to orbit, you will see the marker on the launchpad. This means that you 
                //won't be able to launch without recovering it and if that player release the control lock,
                //you will be recovering a valid vessel that is already in space.
                DelayedClearVessels();
            }
        }

        /// <summary>
        /// This coroutine removes the vessels when switching to the KSC. We delay the removal of the vessels so 
        /// in case we recover a vessel while in flight we correctly recover the crew, funds etc
        /// </summary>
        private static void DelayedClearVessels()
        {
            CoroutineUtil.StartDelayedRoutine(nameof(DelayedClearVessels), () =>
            {
                FlightGlobals.Vessels.Clear();
                HighLogic.CurrentGame?.flightState?.protoVessels?.Clear();
            }, 3);
        }
    }
}
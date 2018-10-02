using LmpClient.Base;
using LmpClient.Localization;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpClient.VesselUtilities;
using System;
using UniLinq;

namespace LmpClient.Systems.VesselRemoveSys
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
            //Only send the vessel remove msg if we own the unloaded update lock
            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(dyingVessel.id, SettingsSystem.CurrentSettings.PlayerName) || dyingVessel.id == _recoveringTerminatingVesselId)
            {
                var ownVesselDying = FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == dyingVessel.id;

                var reason = dyingVessel.id == _recoveringTerminatingVesselId ? "Recovered/Terminated" : "Destroyed";
                LunaLog.Log($"[LMP]: Removing vessel {dyingVessel.id}-{dyingVessel.persistentId}, Name: {dyingVessel.vesselName} from the server: {reason}");

                if (!ownVesselDying)
                {
                    //Add to the kill list so it's also removed from the store later on!
                    System.AddToKillList(dyingVessel, "OnVesselWillDestroy - " + reason);
                    System.MessageSender.SendVesselRemove(dyingVessel);
                }
                else
                {
                    //We do not add our OWN vessel to the kill list as then if we revert we won't be able to send the vessel proto again
                    //As the "VesselWillBeKilled" method will return true.
                    //For this reason we also tell the other players to NOT keep it in the remove list
                    System.MessageSender.SendVesselRemove(dyingVessel, false);
                }
                
                //Vessel is dead so remove the locks after 1500ms to get the debris locks if any
                LockSystem.Singleton.ReleaseAllVesselLocks(dyingVessel.GetVesselCrew().Select(c => c.name), dyingVessel.id, 1500);
            }
        }

        /// <summary>
        /// This event is called when requesting a recovery FROM FLIGHT
        /// </summary>
        public void OnVesselRecovering(Vessel recoveredVessel)
        {
            OnVesselRecovered(recoveredVessel.protoVessel, false);
        }

        /// <summary>
        /// This event is called when the vessel is recovered
        /// </summary>
        public void OnVesselRecovered(ProtoVessel recoveredVessel, bool quick)
        {
            //quick == true when you press "space center" from the inflight menu

            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(recoveredVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.CannotRecover, 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = recoveredVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {recoveredVessel.vesselID}, Name: {recoveredVessel.vesselName} from the server: Recovered");

            System.MessageSender.SendVesselRemove(recoveredVessel.vesselID);

            //Vessel is recovered so remove the locks
            LockSystem.Singleton.ReleaseAllVesselLocks(recoveredVessel.GetVesselCrew().Select(c => c.name), recoveredVessel.vesselID);

            //We consider this vessel removed but we let KSP do the remove of the vessel
            System.RemovedVessels.TryAdd(recoveredVessel.vesselID, DateTime.Now);
        }

        /// <summary>
        /// This event is called when vessel is terminated from track station
        /// </summary>
        public void OnVesselTerminated(ProtoVessel terminatedVessel)
        {
            if (!LockSystem.LockQuery.CanRecoverOrTerminateTheVessel(terminatedVessel.vesselID, SettingsSystem.CurrentSettings.PlayerName))
            {
                LunaScreenMsg.PostScreenMessage(LocalizationContainer.ScreenText.CannotTerminate, 5f, ScreenMessageStyle.UPPER_CENTER);
                return;
            }

            _recoveringTerminatingVesselId = terminatedVessel.vesselID;
            LunaLog.Log($"[LMP]: Removing vessel {terminatedVessel.vesselID}, Name: {terminatedVessel.vesselName} from the server: Terminated");

            System.MessageSender.SendVesselRemove(terminatedVessel.vesselID);

            //Vessel is terminated so remove locks            
            LockSystem.Singleton.ReleaseAllVesselLocks(terminatedVessel.GetVesselCrew().Select(c => c.name), terminatedVessel.vesselID);
            
            //We consider this vessel removed but we let KSP do the remove of the vessel
            System.RemovedVessels.TryAdd(terminatedVessel.vesselID, DateTime.Now);
        }

        /// <summary>
        /// Triggered when reverting back to the launchpad. The vessel id does NOT change
        /// </summary>
        public void OnRevertToLaunch()
        {
            if (FlightGlobals.ActiveVessel != null && !VesselCommon.IsSpectating)
            {
                LunaLog.Log("[LMP]: Detected a revert to launch!");
                RemoveOldVesselAndItsDebris(FlightGlobals.ActiveVessel, ProtoCrewMember.RosterStatus.Assigned);
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
                RemoveOldVesselAndItsDebris(FlightGlobals.ActiveVessel, ProtoCrewMember.RosterStatus.Available);
                System.MessageSender.SendVesselRemove(FlightGlobals.ActiveVessel);

                //We consider this vessel removed but we let KSP do the remove of the vessel
                System.RemovedVessels.TryAdd(FlightGlobals.ActiveVessel.id, DateTime.Now);
            }
        }

        private static void RemoveOldVesselAndItsDebris(Vessel vessel, ProtoCrewMember.RosterStatus kerbalStatus)
        {
            if (vessel == null) return;

            if (FlightGlobals.ActiveVessel.isEVA)
            {
                var kerbal = HighLogic.CurrentGame.CrewRoster[FlightGlobals.ActiveVessel.vesselName];
                if (kerbal != null)
                    kerbal.rosterStatus = kerbalStatus;

                System.AddToKillList(FlightGlobals.ActiveVessel, "Revert. Active vessel is a kerbal");
                System.MessageSender.SendVesselRemove(FlightGlobals.ActiveVessel);
            }

            //We detected a revert, now pick all the vessel parts (debris) that came from our main active 
            //vessel and remove them both from our game and server
            var vesselsToRemove = FlightGlobals.Vessels
                .Where(v => v!= null && v.rootPart?.missionID == vessel.rootPart.missionID && v.id != vessel.id).Distinct();

            foreach (var vesselToRemove in vesselsToRemove)
            {
                if (vesselToRemove.isEVA)
                {
                    var kerbal = HighLogic.CurrentGame.CrewRoster[vesselToRemove.vesselName];
                    if (kerbal != null)
                        kerbal.rosterStatus = kerbalStatus;
                }

                System.MessageSender.SendVesselRemove(vesselToRemove);

                //We consider this vessel removed but we let KSP do the remove of the vessel
                System.RemovedVessels.TryAdd(vesselToRemove.id, DateTime.Now);
            }
        }
    }
}

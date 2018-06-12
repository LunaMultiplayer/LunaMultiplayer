using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.VesselStore;
using LunaClient.VesselUtilities;
using LunaCommon.Time;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        private static Guid _dominantVesselId;
        private static Guid _weakVesselId;

        /// <summary>
        /// Called when 2 parts couple
        /// </summary>
        public void OnPartCouple(GameEvents.FromToAction<Part, Part> partAction)
        {
            if (VesselCommon.IsSpectating || partAction.from.vessel.isEVA || partAction.to.vessel.isEVA) return;

            _dominantVesselId = Vessel.GetDominantVessel(partAction.from.vessel, partAction.to.vessel).id;
            _weakVesselId = partAction.from.vessel.id == _dominantVesselId ? partAction.to.vessel.id : partAction.from.vessel.id;
        }

        /// <summary>
        /// This event is called AFTER all the docking event is over, so the final 
        /// vessel is merged and we can safely remove the minor vessel
        /// </summary>
        public void OnVesselWasModified(Vessel data)
        {
            if (data.id == _dominantVesselId)
            {
                var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;

                if (_dominantVesselId == FlightGlobals.ActiveVessel?.id)
                {
                    JumpIfVesselOwnerIsInFuture(_weakVesselId);

                    LunaLog.Log($"[LMP]: Docking detected! We own the dominant vessel {_dominantVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(_weakVesselId, "Killing weak vessel during a docking");

                    System.MessageSender.SendDockInformation(_weakVesselId, FlightGlobals.ActiveVessel, currentSubspaceId);
                }
                else if (_weakVesselId == FlightGlobals.ActiveVessel?.id)
                {
                    JumpIfVesselOwnerIsInFuture(_dominantVesselId);

                    LunaLog.Log($"[LMP]: Docking detected! We DON'T own the dominant vessel {_dominantVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(_weakVesselId, "Killing weak (active) vessel during a docking");

                    //Switch to the dominant vessel, but before that save the dominant vessel proto.
                    //We save it as in case the dominant player didn't detected the dock he will send us a
                    //NOT docked protovessel and that would remove the weak vessel because we are going to be an
                    //spectator...
                    VesselSwitcherSystem.Singleton.SwitchToVessel(_dominantVesselId);
                    MainSystem.Singleton.StartCoroutine(WaitUntilWeSwitchedThenSendDockInfo(_weakVesselId, _dominantVesselId));

                }
            }

            _dominantVesselId = Guid.Empty;
            _weakVesselId = Guid.Empty;
        }

        /// <summary>
        /// Event called after the undocking is completed and we have the 2 final vessels
        /// </summary>
        public void UndockingComplete(Vessel vessel1, Vessel vessel2)
        {
            if (VesselCommon.IsSpectating) return;

            LunaLog.Log("Undock detected!");

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel1, true, true);
            VesselsProtoStore.AddOrUpdateVesselToDictionary(vessel1);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel2, true, true);
            VesselsProtoStore.AddOrUpdateVesselToDictionary(vessel2);

            //Release the locks of the vessel we are not in
            var crewToReleaseLocks = FlightGlobals.ActiveVessel?.id == vessel1.id
                ? vessel2.GetVesselCrew().Select(c => c.name)
                : vessel1.GetVesselCrew().Select(c => c.name);

            LockSystem.Singleton.ReleaseAllVesselLocks(crewToReleaseLocks, FlightGlobals.ActiveVessel?.id == vessel1.id ? vessel2.id : vessel1.id);

            LunaLog.Log($"Undocking finished. Vessels: {vessel1.id} and {vessel2.id}");
        }

        #region Private
        
        /// <summary>
        /// Jumps to the subspace of the controller vessel in case he is more advanced in time
        /// </summary>
        private static void JumpIfVesselOwnerIsInFuture(Guid vesselId)
        {
            var dominantVesselOwner = LockSystem.LockQuery.GetControlLockOwner(vesselId);
            if (dominantVesselOwner != null)
            {
                var dominantVesselOwnerSubspace = WarpSystem.Singleton.GetPlayerSubspace(dominantVesselOwner);
                WarpSystem.Singleton.WarpIfSubspaceIsMoreAdvanced(dominantVesselOwnerSubspace);
            }
        }

        /// <summary>
        /// Here we wait until we fully switched to the dominant vessel and THEN we send the vessel dock information.
        /// We wait 5 seconds before sending the data to give time to the dominant vessel to detect the dock
        /// </summary>
        private static IEnumerator WaitUntilWeSwitchedThenSendDockInfo(Guid weakId, Guid dominantId, int secondsToWait = 5)
        {
            var start = LunaComputerTime.UtcNow;
            var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;
            var waitInterval = new WaitForSeconds(0.5f);

            while (FlightGlobals.ActiveVessel?.id != dominantId && LunaComputerTime.UtcNow - start < TimeSpan.FromSeconds(30))
            {
                yield return waitInterval;
            }

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == dominantId)
            {
                /* We are NOT the dominant vessel so wait 5 seconds so the dominant vessel detects the docking.
                 * If we send the vessel definition BEFORE the dominant detects it, then the dominant won't be able
                 * to undock properly as he will think that he is the weak vessel.
                 */

                yield return new WaitForSeconds(secondsToWait);

                FlightGlobals.ActiveVessel.BackupVessel();
                LunaLog.Log($"[LMP]: Sending dock info to the server! Final dominant vessel parts {FlightGlobals.ActiveVessel.protoVessel.protoPartSnapshots.Count}");

                System.MessageSender.SendDockInformation(weakId, FlightGlobals.FindVessel(dominantId), currentSubspaceId, FlightGlobals.ActiveVessel.protoVessel);
            }
        }

        #endregion
    }
}

using LunaClient.Base;
using LunaClient.Extensions;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
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
        private static uint _dominantVesselPersistentId;
        private static uint _weakVesselPersistentId;

        /// <summary>
        /// Called when 2 parts couple
        /// </summary>
        public void OnPartCouple(GameEvents.FromToAction<Part, Part> partAction)
        {
            if (VesselCommon.IsSpectating || partAction.from.vessel.isEVA || partAction.to.vessel.isEVA) return;

            var dominantVessel = Vessel.GetDominantVessel(partAction.from.vessel, partAction.to.vessel);
            _dominantVesselId = dominantVessel.id;
            _dominantVesselPersistentId = dominantVessel.persistentId;

            var weakVessel = partAction.from.vessel.persistentId == _dominantVesselPersistentId ? partAction.to.vessel : partAction.from.vessel;
            _weakVesselId = weakVessel.id;
            _weakVesselPersistentId = weakVessel.persistentId;
        }

        /// <summary>
        /// This event is called AFTER all the docking event is over, so the final 
        /// vessel is merged and we can safely remove the minor vessel
        /// </summary>
        public void OnVesselWasModified(Vessel vesselModified)
        {
            if (vesselModified.id == _dominantVesselId)
            {
                var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;

                if (_dominantVesselId == FlightGlobals.ActiveVessel?.id)
                {
                    JumpIfVesselOwnerIsInFuture(_weakVesselId);

                    LunaLog.Log($"[LMP]: Docking detected! We own the dominant vessel {_dominantVesselId}");
                    var vesselToKill = FlightGlobals.fetch.FindVessel(_weakVesselPersistentId, _weakVesselId);
                    if (vesselToKill != null)
                    {
                        VesselRemoveSystem.Singleton.AddToKillList(vesselToKill, "Killing weak vessel during a docking");
                    }

                    System.MessageSender.SendDockInformation(_weakVesselId, _weakVesselPersistentId, FlightGlobals.ActiveVessel, currentSubspaceId);
                }
                else if (_weakVesselId == FlightGlobals.ActiveVessel?.id)
                {
                    JumpIfVesselOwnerIsInFuture(_dominantVesselId);

                    LunaLog.Log($"[LMP]: Docking detected! We DON'T own the dominant vessel {_dominantVesselId}");
                    var vesselToKill = FlightGlobals.fetch.FindVessel(_weakVesselPersistentId, _weakVesselId);
                    if (vesselToKill != null)
                    {
                        VesselRemoveSystem.Singleton.AddToKillList(vesselToKill, "Killing weak (active) vessel during a docking");
                    }

                    //Switch to the dominant vessel, but before that save the dominant vessel proto.
                    //We save it as in case the dominant player didn't detected the dock he will send us a
                    //NOT docked protovessel and that would remove the weak vessel because we are going to be an
                    //spectator...
                    VesselSwitcherSystem.Singleton.SwitchToVessel(FlightGlobals.fetch.FindVessel(_dominantVesselPersistentId, _dominantVesselId));
                    MainSystem.Singleton.StartCoroutine(WaitUntilWeSwitchedThenSendDockInfo(_weakVesselId, _weakVesselPersistentId, _dominantVesselPersistentId));
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

            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel1, true);
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel2, true);

            //Release the locks of the vessel we are not in
            var crewToReleaseLocks = FlightGlobals.ActiveVessel == vessel1
                ? vessel2.GetVesselCrew().Select(c => c.name)
                : vessel1.GetVesselCrew().Select(c => c.name);

            var vesselToRelease = FlightGlobals.ActiveVessel == vessel1 ? vessel2 : vessel1;
            LockSystem.Singleton.ReleaseAllVesselLocks(crewToReleaseLocks, vesselToRelease.id, vesselToRelease.persistentId);

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
        private static IEnumerator WaitUntilWeSwitchedThenSendDockInfo(Guid weakId, uint weakPersistantId, uint dominantPersistentId, int secondsToWait = 5)
        {
            var start = LunaComputerTime.UtcNow;
            var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;
            var waitInterval = new WaitForSeconds(0.5f);

            while (FlightGlobals.ActiveVessel?.persistentId != dominantPersistentId && LunaComputerTime.UtcNow - start < TimeSpan.FromSeconds(30))
            {
                yield return waitInterval;
            }

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.persistentId == dominantPersistentId)
            {
                /* We are NOT the dominant vessel so wait 5 seconds so the dominant vessel detects the docking.
                 * If we send the vessel definition BEFORE the dominant detects it, then the dominant won't be able
                 * to undock properly as he will think that he is the weak vessel.
                 */

                yield return new WaitForSeconds(secondsToWait);

                FlightGlobals.ActiveVessel.BackupVessel();
                LunaLog.Log($"[LMP]: Sending dock info to the server! Final dominant vessel parts {FlightGlobals.ActiveVessel.protoVessel.protoPartSnapshots.Count}");

                System.MessageSender.SendDockInformation(weakId, weakPersistantId, FlightGlobals.ActiveVessel, currentSubspaceId, FlightGlobals.ActiveVessel.protoVessel);
            }
        }

        #endregion
    }
}

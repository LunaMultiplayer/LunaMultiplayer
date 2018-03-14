using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        /// <summary>
        /// This dictioanry stores the docking events
        /// </summary>
        private static Dictionary<Guid, VesselDockStructure> VesselDockings { get; } = new Dictionary<Guid, VesselDockStructure>();

        /// <summary>
        /// Called when 2 parts couple
        /// </summary>
        /// <param name="partAction"></param>
        public void OnPartCouple(GameEvents.FromToAction<Part, Part> partAction)
        {
            if (!VesselCommon.IsSpectating)
            {
                if (partAction.from.vessel != null && partAction.to.vessel != null)
                {
                    var dock = new VesselDockStructure(partAction.from.vessel.id, partAction.to.vessel.id);
                    if (dock.StructureIsOk())
                    {
                        if (partAction.from.Modules.Contains("KerbalSeat") || partAction.to.Modules.Contains("KerbalSeat"))
                        {
                            HandleExternalSeatBoard(dock);
                        }
                        else
                        {
                            //We add it to the event so the event is handled AFTER all the docking event in ksp is over and we can
                            //safely remove the weak vessel from the game and save the updated dominant vessel.
                            VesselDockings.Add(dock.DominantVesselId, dock);
                        }
                    }
                }
            }
            else
            {
                LunaLog.Log("[LMP]: Spectator docking happened. This needs to be fixed later.");
            }
        }

        public void OnCrewBoard(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Log("[LMP]: Crew boarding detected!");
            if (!VesselCommon.IsSpectating)
            {
                LunaLog.Log($"[LMP]: EVA Boarding, from: {partAction.from.vessel.id }, Name: {partAction.from.vessel.vesselName}");
                LunaLog.Log($"[LMP]: EVA Boarding, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");

                var dock = new VesselDockStructure(partAction.from.vessel.id, partAction.to.vessel.id);
                if (dock.StructureIsOk())
                {
                    HandleDocking(dock, true);
                }
            }
        }

        public void OnPartUndock(Part data)
        {
            if (VesselCommon.IsSpectating)
            {
                FlightCamera.SetTarget(data.vessel);
                data.vessel.MakeActive();
            }
        }

        /// <summary>
        /// This event is called AFTER all the docking event is over, so the final 
        /// vessel is merged and we can safely remove the minor vessel
        /// </summary>
        public void OnVesselWasModified(Vessel data)
        {
            if (VesselDockings.ContainsKey(data.id))
            {
                HandleDocking(VesselDockings[data.id], false);
                VesselDockings.Remove(data.id);
            }
        }

        /// <summary>
        /// Here we handle the "dock" when a kerbal goes into an external seat
        /// </summary>
        private static void HandleExternalSeatBoard(VesselDockStructure dock)
        {
            var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;

            //Kerbal must never be the dominant
            var temp = dock.DominantVesselId;
            dock.DominantVesselId = dock.WeakVesselId;
            dock.WeakVesselId = temp;

            LunaLog.Log($"[LMP]: Crewboard to an external seat detected! We own the kerbal {dock.WeakVesselId}");
            VesselRemoveSystem.Singleton.AddToKillList(dock.WeakVesselId);

            dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);
            System.MessageSender.SendDockInformation(dock, currentSubspaceId);
            MainSystem.Singleton.StartCoroutine(WaitUntilWeSwitchedThenSendDockInfo(dock));
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewTransfered(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel, true);
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(data.from.vessel, true);
        }

        #region Private

        /// <summary>
        /// This method is called after the docking is over and there 
        /// should be only 1 vessel in the screen (the final one)
        /// </summary>
        private static void HandleDocking(VesselDockStructure dock, bool eva)
        {
            var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;

            if (dock.DominantVesselId == FlightGlobals.ActiveVessel?.id)
            {
                JumpIfVesselOwnerIsInFuture(dock.WeakVesselId);
                if (eva)
                {
                    //The kerbal should never be the dominant!
                    var temp = dock.DominantVesselId;
                    dock.DominantVesselId = dock.WeakVesselId;
                    dock.WeakVesselId = temp;

                    LunaLog.Log($"[LMP]: Crewboard detected! We own the kerbal {dock.WeakVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(dock.WeakVesselId);

                    dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);
                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Docking detected! We own the dominant vessel {dock.DominantVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(dock.WeakVesselId);
                    dock.DominantVessel = FlightGlobals.ActiveVessel;

                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
            }
            else if (dock.WeakVesselId == FlightGlobals.ActiveVessel?.id)
            {
                JumpIfVesselOwnerIsInFuture(dock.DominantVesselId);
                if (eva)
                {
                    //The kerbal should never be the dominant!
                    var temp = dock.DominantVesselId;
                    dock.DominantVesselId = dock.WeakVesselId;
                    dock.WeakVesselId = temp;

                    LunaLog.Log($"[LMP]: Crewboard detected! We own the vessel {dock.DominantVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(dock.WeakVesselId);

                    dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);
                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Docking detected! We DON'T own the dominant vessel {dock.DominantVesselId}");
                    VesselRemoveSystem.Singleton.AddToKillList(dock.WeakVesselId);

                    if (dock.DominantVessel == null)
                        dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);

                    //Switch to the dominant vessel, but before that save the dominant vessel proto.
                    //We save it as in case the dominant player didn't detected the dock he will send us a
                    //NOT docked protovessel and that would remove the weak vessel because we are going to be an
                    //spectator...
                    VesselSwitcherSystem.Singleton.SwitchToVessel(dock.DominantVesselId);
                    MainSystem.Singleton.StartCoroutine(WaitUntilWeSwitchedThenSendDockInfo(dock));
                }
            }
        }

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
        private static IEnumerator WaitUntilWeSwitchedThenSendDockInfo(VesselDockStructure dockInfo, int secondsToWait = 5)
        {
            var start = DateTime.Now;
            var currentSubspaceId = WarpSystem.Singleton.CurrentSubspace;
            var waitInterval = new WaitForSeconds(0.5f);

            while (FlightGlobals.ActiveVessel?.id != dockInfo.DominantVesselId && DateTime.Now - start < TimeSpan.FromSeconds(30))
            {
                yield return waitInterval;
            }

            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.id == dockInfo.DominantVesselId)
            {
                /* We are NOT the dominant vessel so wait 5 seconds so the dominant vessel detects the docking.
                 * If we send the vessel definition BEFORE the dominant detects it, then the dominant won't be able
                 * to undock properly as he will think that he is the weak vessel.
                 */

                yield return new WaitForSeconds(secondsToWait);

                FlightGlobals.ActiveVessel.BackupVessel();
                LunaLog.Log($"[LMP]: Sending dock info to the server! Final dominant vessel parts {FlightGlobals.ActiveVessel.protoVessel.protoPartSnapshots.Count}");

                System.MessageSender.SendDockInformation(dockInfo, currentSubspaceId, FlightGlobals.ActiveVessel.protoVessel);
            }
        }

        #endregion
    }
}
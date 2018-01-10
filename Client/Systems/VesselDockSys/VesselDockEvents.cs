using LunaClient.Base;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using LunaClient.Systems.Warp;
using LunaClient.VesselUtilities;
using System;
using System.Collections.Generic;
using LunaClient.Systems.VesselProtoSys;

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
                        //We add it to the event so the event is handled AFTER all the docking event in ksp is over and we can
                        //safely remove the weak vessel from the game and save the updated dominant vessel.
                        VesselDockings.Add(dock.DominantVesselId, dock);
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
        /// This method is called after the docking is over and there 
        /// should be only 1 vessel in the screen (the final one)
        /// </summary>
        private static void HandleDocking(VesselDockStructure dock, bool eva)
        {
            var currentSubspaceId = SystemsContainer.Get<WarpSystem>().CurrentSubspace;

            if (dock.DominantVesselId == FlightGlobals.ActiveVessel?.id)
            {
                if (eva)
                {
                    LunaLog.Log($"[LMP]: Crewboard detected! We own the kerbal {dock.DominantVesselId}");
                    SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.DominantVesselId);
                    
                    //The kerbal should never be the dominant!
                    var temp = dock.DominantVesselId;
                    dock.DominantVesselId = dock.WeakVesselId;
                    dock.WeakVesselId = temp;

                    dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);

                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Docking detected! We own the dominant vessel {dock.DominantVesselId}");
                    SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.WeakVesselId);
                    dock.DominantVessel = FlightGlobals.ActiveVessel;

                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
            }
            else if (dock.WeakVesselId == FlightGlobals.ActiveVessel?.id)
            {
                if (eva)
                {
                    LunaLog.Log($"[LMP]: Crewboard detected! We own the vessel {dock.WeakVesselId}");
                    SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.DominantVesselId);

                    //The kerbal should never be the dominant!
                    var temp = dock.DominantVesselId;
                    dock.DominantVesselId = dock.WeakVesselId;
                    dock.WeakVesselId = temp;

                    dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);

                    System.MessageSender.SendDockInformation(dock, currentSubspaceId);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Docking detected! We DON'T own the dominant vessel {dock.DominantVesselId}");
                    SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.WeakVesselId);

                    if (dock.DominantVessel == null)
                        dock.DominantVessel = FlightGlobals.FindVessel(dock.DominantVesselId);

                    //Switch to the dominant vessel
                    SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(dock.DominantVesselId);

                    /* We are NOT the dominant vessel so wait 5 seconds so the dominant vessel detects the docking.
                     * If we send the vessel definition BEFORE the dominant detects it, then the dominant won't be able
                     * to undock properly as he will think that he is the weak vessel.
                     */
                    System.MessageSender.SendDockInformation(dock, currentSubspaceId, 5);
                }
            }
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewTransfered(GameEvents.HostedFromToAction<ProtoCrewMember, Part> data)
        {
            SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(data.from.vessel);
        }

        /// <summary>
        /// The vessel has changed as it has less crew now so send the definition
        /// </summary>
        public void OnCrewEva(GameEvents.FromToAction<Part, Part> data)
        {
            SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(data.from.vessel);
        }
    }
}
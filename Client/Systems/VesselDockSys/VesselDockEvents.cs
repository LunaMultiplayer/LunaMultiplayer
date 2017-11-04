using LunaClient.Base;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Systems.VesselSwitcherSys;
using System;
using System.Collections.Generic;

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
                    HandleDocking(dock);
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
                HandleDocking(VesselDockings[data.id]);
                VesselDockings.Remove(data.id);
            }
        }

        /// <summary>
        /// This method is called after the docking is over and there 
        /// should be only 1 vessel in the screen (the final one)
        /// </summary>
        private static void HandleDocking(VesselDockStructure dock)
        {
            if (dock.DominantVesselId == FlightGlobals.ActiveVessel?.id)
            {
                LunaLog.Log($"[LMP]: Docking detected! We own the dominant vessel {dock.DominantVesselId}");

                //Backup the vesselproto with the docked vessel data
                FlightGlobals.ActiveVessel.BackupVessel();
                dock.DominantVessel = FlightGlobals.ActiveVessel;

                System.MessageSender.SendDockInformation(dock);
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.WeakVessel);
            }
            else if (dock.WeakVesselId == FlightGlobals.ActiveVessel?.id)
            {
                LunaLog.Log($"[LMP]: Docking detected! We DON'T own the dominant vessel {dock.DominantVesselId}");

                //Switch to the dominant vessel
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(dock.DominantVesselId);

                System.MessageSender.SendDockInformation(dock);
                SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.WeakVessel);
            }
        }
    }
}
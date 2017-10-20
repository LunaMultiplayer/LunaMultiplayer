using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
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
        /// 
        /// </summary>
        /// <param name="partAction"></param>
        public void OnPartCouple(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Log("[LMP]: Vessel docking detected!");
            if (!VesselCommon.IsSpectating)
            {
                if (partAction.from.vessel != null && partAction.to.vessel != null)
                {
                    var fromVesselUpdateLockExists = LockSystem.LockQuery.UpdateLockExists(partAction.from.vessel.id);
                    var toVesselUpdateLockExists = LockSystem.LockQuery.UpdateLockExists(partAction.to.vessel.id);
                    var fromVesselUpdateLockIsOurs = LockSystem.LockQuery.UpdateLockBelongsToPlayer(partAction.from.vessel.id,
                        SettingsSystem.CurrentSettings.PlayerName);
                    var toVesselUpdateLockIsOurs = LockSystem.LockQuery.UpdateLockBelongsToPlayer(partAction.to.vessel.id,
                        SettingsSystem.CurrentSettings.PlayerName);

                    if (fromVesselUpdateLockIsOurs || toVesselUpdateLockIsOurs || !fromVesselUpdateLockExists || !toVesselUpdateLockExists)
                    {
                        if (FlightGlobals.ActiveVessel != null)
                            LunaLog.Log($"[LMP]: Vessel docking, our vessel: {VesselCommon.CurrentVesselId}");

                        LunaLog.Log($"[LMP]: Vessel docking, from: {partAction.from.vessel.id}, Name: {partAction.from.vessel.vesselName}");
                        LunaLog.Log($"[LMP]: Vessel docking, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");

                        var dock = new VesselDockStructure(partAction.from.vessel.id, partAction.to.vessel.id);
                        if (dock.StructureIsOk())
                        {
                            //We add it to the event so the event is handled AFTER all the docking event in ksp is over and we can
                            //safely remove the Minorvessel from the game and save the new dominant vessel as a proto.
                            VesselDockings.Add(dock.DominantVesselId, dock);
                        }
                    }
                    else
                    {
                        LunaLog.Log("[LMP]: Inconsistent docking state detected, killing both vessels if possible.");
                        if (partAction.from.vessel != FlightGlobals.ActiveVessel)
                            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(partAction.from.vessel, true);
                        if (partAction.to.vessel != FlightGlobals.ActiveVessel)
                            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(partAction.to.vessel, true);
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
            if (dock.DominantVessel == FlightGlobals.ActiveVessel)
            {
                LunaLog.Log($"[LMP]: Docking: We own the dominant vessel {dock.DominantVesselId}");

                FlightGlobals.ActiveVessel.BackupVessel();
                SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(FlightGlobals.ActiveVessel.protoVessel);
            }
            else
            {
                LunaLog.Log($"[LMP]: Docking: We DON'T own the dominant vessel {dock.DominantVesselId}. Switching");
                SystemsContainer.Get<VesselSwitcherSystem>().SwitchToVessel(dock.DominantVesselId);
            }

            SystemsContainer.Get<VesselRemoveSystem>().AddToKillList(dock.MinorVessel, true);
            SystemsContainer.Get<VesselRemoveSystem>().MessageSender.SendVesselRemove(dock.MinorVesselId, true);
            LunaLog.Log("[LMP]: Docking event over!");
        }
    }
}
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselRemoveSys;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        public void OnVesselDock(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Log("[LMP]: Vessel docking detected!");
            if (!VesselCommon.IsSpectating)
            {
                if (partAction.from.vessel != null && partAction.to.vessel != null)
                {
                    var fromVesselUpdateLockExists = SystemsContainer.Get<LockSystem>().LockExists($"update-{partAction.from.vessel.id}");
                    var toVesselUpdateLockExists = SystemsContainer.Get<LockSystem>().LockExists($"update-{partAction.to.vessel.id}");
                    var fromVesselUpdateLockIsOurs = SystemsContainer.Get<LockSystem>().LockIsOurs($"update-{partAction.from.vessel.id}");
                    var toVesselUpdateLockIsOurs = SystemsContainer.Get<LockSystem>().LockIsOurs($"update-{partAction.to.vessel.id}");

                    if (fromVesselUpdateLockIsOurs || toVesselUpdateLockIsOurs || !fromVesselUpdateLockExists || !toVesselUpdateLockExists)
                    {
                        if (FlightGlobals.ActiveVessel != null)
                            LunaLog.Log($"[LMP]: Vessel docking, our vessel: {VesselCommon.CurrentVesselId}");

                        LunaLog.Log($"[LMP]: Vessel docking, from: {partAction.from.vessel.id}, Name: {partAction.from.vessel.vesselName}");
                        LunaLog.Log($"[LMP]: Vessel docking, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");

                        System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
                    }
                    else
                    {
                        LunaLog.Log("[LMP]: Inconsistent docking state detected, killing both vessels if possible.");
                        if (partAction.from.vessel != FlightGlobals.ActiveVessel)
                            SystemsContainer.Get<VesselRemoveSystem>().KillVessel(partAction.from.vessel, true);
                        if (partAction.to.vessel != FlightGlobals.ActiveVessel)
                            SystemsContainer.Get<VesselRemoveSystem>().KillVessel(partAction.to.vessel, true);
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

                System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
            }
        }

        public void OnVesselUndock(Part data)
        {
            if (VesselCommon.IsSpectating)
            {
                FlightCamera.SetTarget(data.vessel);
                data.vessel.MakeActive();
            }
        }
    }
}
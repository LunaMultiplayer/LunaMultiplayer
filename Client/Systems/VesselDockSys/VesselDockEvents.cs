using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        public void OnVesselDock(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Debug("Vessel docking detected!");
            if (!VesselLockSystem.Singleton.IsSpectating)
            {
                if ((partAction.from.vessel != null) && (partAction.to.vessel != null))
                {
                    var fromVesselUpdateLockExists = LockSystem.Singleton.LockExists("update-" + partAction.from.vessel.id);
                    var toVesselUpdateLockExists = LockSystem.Singleton.LockExists("update-" + partAction.to.vessel.id);
                    var fromVesselUpdateLockIsOurs = LockSystem.Singleton.LockIsOurs("update-" + partAction.from.vessel.id);
                    var toVesselUpdateLockIsOurs = LockSystem.Singleton.LockIsOurs("update-" + partAction.to.vessel.id);

                    if (fromVesselUpdateLockIsOurs || toVesselUpdateLockIsOurs || !fromVesselUpdateLockExists || !toVesselUpdateLockExists)
                    {
                        if (FlightGlobals.ActiveVessel != null)
                            LunaLog.Debug($"Vessel docking, our vessel: {VesselCommon.CurrentVesselId}");

                        LunaLog.Debug($"Vessel docking, from: {partAction.from.vessel.id}, Name: {partAction.from.vessel.vesselName}");
                        LunaLog.Debug($"Vessel docking, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");
                        
                        System.PrintDockingInProgress();
                        System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
                    }
                    else
                    {
                        LunaLog.Debug("Inconsistent docking state detected, killing both vessels if possible.");
                        if (partAction.from.vessel != FlightGlobals.ActiveVessel)
                            VesselRemoveSystem.Singleton.KillVessel(partAction.from.vessel);
                        if (partAction.to.vessel != FlightGlobals.ActiveVessel)
                            VesselRemoveSystem.Singleton.KillVessel(partAction.to.vessel);
                    }
                }
            }
            else
            {
                LunaLog.Debug("Spectator docking happened. This needs to be fixed later.");
            }
        }

        public void OnCrewBoard(GameEvents.FromToAction<Part, Part> partAction)
        {
            LunaLog.Debug("Crew boarding detected!");
            if (!VesselLockSystem.Singleton.IsSpectating)
            {
                LunaLog.Debug($"EVA Boarding, from: {partAction.from.vessel.id }, Name: {partAction.from.vessel.vesselName}");
                LunaLog.Debug($"EVA Boarding, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");

                System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
            }
        }
    }
}
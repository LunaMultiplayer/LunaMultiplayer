using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.VesselLockSys;
using LunaClient.Systems.VesselRemoveSys;
using LunaClient.Utilities;
using UnityEngine;

namespace LunaClient.Systems.VesselDockSys
{
    public class VesselDockEvents : SubSystem<VesselDockSystem>
    {
        public void OnVesselDock(GameEvents.FromToAction<Part, Part> partAction)
        {
            Debug.Log("Vessel docking detected!");
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
                            Debug.Log($"Vessel docking, our vessel: {VesselCommon.CurrentVesselId}");

                        Debug.Log($"Vessel docking, from: {partAction.from.vessel.id}, Name: {partAction.from.vessel.vesselName}");
                        Debug.Log($"Vessel docking, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");
                        
                        System.PrintDockingInProgress();
                        System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
                    }
                    else
                    {
                        Debug.Log("Inconsistent docking state detected, killing both vessels if possible.");
                        if (partAction.from.vessel != FlightGlobals.ActiveVessel)
                            VesselRemoveSystem.Singleton.KillVessel(partAction.from.vessel);
                        if (partAction.to.vessel != FlightGlobals.ActiveVessel)
                            VesselRemoveSystem.Singleton.KillVessel(partAction.to.vessel);
                    }
                }
            }
            else
            {
                Debug.Log("Spectator docking happened. This needs to be fixed later.");
            }
        }

        public void OnCrewBoard(GameEvents.FromToAction<Part, Part> partAction)
        {
            Debug.Log("Crew boarding detected!");
            if (!VesselLockSystem.Singleton.IsSpectating)
            {
                Debug.Log($"EVA Boarding, from: {partAction.from.vessel.id }, Name: {partAction.from.vessel.vesselName}");
                Debug.Log($"EVA Boarding, to: {partAction.to.vessel.id}, Name: {partAction.to.vessel.vesselName}");

                System.HandleDocking(partAction.from.vessel.id, partAction.to.vessel.id);
            }
        }
    }
}
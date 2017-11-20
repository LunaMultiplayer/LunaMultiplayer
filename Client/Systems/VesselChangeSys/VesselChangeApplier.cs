using System.Linq;

namespace LunaClient.Systems.VesselChangeSys
{
    /// <summary>
    /// Here we handle the changes that a vessel has received. A "change" is a antenna that is deployed, a shield that is opened, etc.
    /// </summary>
    public class VesselChangeApplier
    {
        /// <summary>
        /// This method applies the changes we collected to a given vessel
        /// </summary>
        public static void ProcessVesselChanges(Vessel vessel, VesselChange vesselChange)
        {
            if (vessel != null)
            {
                if (vesselChange.Stage > int.MinValue)
                {
                    vessel.ActionGroups?.ToggleGroup(KSPActionGroup.Stage);
                }

                foreach (var partToRemove in vesselChange.PartsToRemove)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == partToRemove);
                    if (part != null)
                    {
                        part.Die();
                    }
                }

                foreach (var partToExtend in vesselChange.PartsToExtend)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == partToExtend);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleDeployablePart>()?.Extend();
                    }
                }

                foreach (var partToExtend in vesselChange.PartsToRetract)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == partToExtend);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleDeployablePart>()?.Retract();
                    }
                }

                foreach (var shieldToClose in vesselChange.ShieldsToClose)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == shieldToClose);
                    if (part != null)
                    {
                        var module = part.FindModuleImplementing<ModuleDockingNode>();
                        if (module != null && !module.IsDisabled && module.deployAnimator != null)
                        {
                            var isClosed = module.deployAnimator.animSwitch;
                            if (!isClosed)
                                module.deployAnimator.Toggle();
                        }
                    }
                }

                foreach (var shieldToClose in vesselChange.ShieldsToOpen)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == shieldToClose);
                    if (part != null)
                    {
                        var module = part.FindModuleImplementing<ModuleDockingNode>();
                        if (module != null && !module.IsDisabled && module.deployAnimator != null)
                        {
                            var isClosed = module.deployAnimator.animSwitch;
                            if (isClosed)
                                module.deployAnimator.Toggle();
                        }
                    }
                }

                foreach (var engineToStart in vesselChange.EnginesToStart)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == engineToStart);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleEngines>()?.Activate();
                    }
                }

                foreach (var engineToStop in vesselChange.EnginesToStop)
                {
                    var part = vessel.parts.FirstOrDefault(p => p.craftID == engineToStop);
                    if (part != null)
                    {
                        part.FindModuleImplementing<ModuleEngines>()?.Shutdown();
                    }
                }

                foreach (var actionGroup in vesselChange.ActionGroupsToEnable)
                {
                    vessel.ActionGroups?.SetGroup(actionGroup, true);
                }

                foreach (var actionGroup in vesselChange.ActionGroupsToDisable)
                {
                    vessel.ActionGroups?.SetGroup(actionGroup, false);
                }
            }
        }
    }
}

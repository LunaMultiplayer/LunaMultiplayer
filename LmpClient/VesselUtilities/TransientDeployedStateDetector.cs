using System.Collections.Generic;

namespace LmpClient.VesselUtilities
{
    public static class TransientDeployedStateDetector
    {
        private static readonly HashSet<string> GroundModuleNames = new HashSet<string>(System.StringComparer.Ordinal)
        {
            "ModuleGroundPart",
            "ModuleGroundSciencePart",
            "ModuleGroundCommsPart",
            "ModuleGroundExperiment",
            "ModuleGroundExpControl",
        };

        public static bool IsGroundModule(string moduleName)
        {
            return GroundModuleNames.Contains(moduleName);
        }

        public static bool IsTransientDeployedModule(string moduleName, bool? beingDeployed, bool? deployedOnGround, bool? isEnabled)
        {
            if (!GroundModuleNames.Contains(moduleName)) return false;
            if (beingDeployed != true) return false;
            return deployedOnGround == false || isEnabled == false;
        }
    }
}

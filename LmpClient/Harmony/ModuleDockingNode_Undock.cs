using HarmonyLib;
using LmpClient.VesselUtilities;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Guard local docking-node undock actions when FSM is in a bad state.
    /// </summary>
    [HarmonyPatch(typeof(ModuleDockingNode))]
    [HarmonyPatch("Undock")]
    public class ModuleDockingNode_Undock
    {
        [HarmonyPrefix]
        private static bool PrefixUndock(ModuleDockingNode __instance)
        {
            if (!DockingPortUtil.EnsureRecoverableForUndock(__instance,
                    "ModuleDockingNode.Undock", out var failureReason))
            {
                LunaLog.LogWarning($"[LMP]: Blocking ModuleDockingNode.Undock. {failureReason}. " +
                    $"Part: {__instance.part?.partName}, Vessel: {__instance.vessel?.id}, " +
                    $"PartFlightId: {__instance.part?.flightID}");
                return false;
            }

            return true;
        }
    }
}

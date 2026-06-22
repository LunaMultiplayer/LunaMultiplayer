using HarmonyLib;
using LmpClient.VesselUtilities;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// Guard same-vessel undock actions when FSM is in a bad state.
    /// </summary>
    [HarmonyPatch(typeof(ModuleDockingNode))]
    [HarmonyPatch("UndockSameVessel")]
    public class ModuleDockingNode_UndockSameVessel
    {
        [HarmonyPrefix]
        private static bool PrefixUndockSameVessel(ModuleDockingNode __instance)
        {
            if (!DockingPortUtil.EnsureRecoverableForUndock(__instance,
                    "ModuleDockingNode.UndockSameVessel", out var failureReason))
            {
                LunaLog.LogWarning($"[LMP]: Blocking ModuleDockingNode.UndockSameVessel. {failureReason}. " +
                    $"Part: {__instance.part?.partName}, Vessel: {__instance.vessel?.id}, " +
                    $"PartFlightId: {__instance.part?.flightID}");
                return false;
            }

            return true;
        }
    }
}

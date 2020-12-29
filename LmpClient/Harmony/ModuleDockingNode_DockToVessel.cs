using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when docking a vessel
    /// </summary>
    [HarmonyPatch(typeof(ModuleDockingNode))]
    [HarmonyPatch("DockToVessel")]
    public class ModuleDockingNode_DockToVessel
    {
        [HarmonyPrefix]
        private static void PrefixDockToVessel(ModuleDockingNode __instance, ModuleDockingNode node)
        {
            VesselDockEvent.onDocking.Fire(__instance.vessel, node.vessel);
        }

        [HarmonyPostfix]
        private static void PostfixDockToVessel(ModuleDockingNode __instance, ModuleDockingNode node)
        {
            VesselDockEvent.onDockingComplete.Fire(__instance.vessel, node.vessel);
        }
    }
}

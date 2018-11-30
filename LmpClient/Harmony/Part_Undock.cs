using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when undocking a part
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("Undock")]
    public class Part_Undock
    {
        [HarmonyPrefix]
        private static void PrefixUndock(Part __instance, DockedVesselInfo newVesselInfo, ref Vessel __state)
        {
            __state = __instance.vessel;
            PartEvent.onPartUndocking.Fire(__instance, newVesselInfo);
        }

        [HarmonyPostfix]
        private static void PostfixUndock(Part __instance, DockedVesselInfo newVesselInfo, ref Vessel __state)
        {
            PartEvent.onPartUndocked.Fire(__instance, newVesselInfo, __state);
        }
    }
}

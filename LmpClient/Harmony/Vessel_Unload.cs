using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when unloading a vessel
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("Unload")]
    public class Vessel_Unload
    {
        [HarmonyPrefix]
        private static void PrefixUnload(Vessel __instance)
        {
            VesselUnloadEvent.onVesselUnloading.Fire(__instance);
        }

        [HarmonyPostfix]
        private static void PostfixUnload(Vessel __instance)
        {
            VesselUnloadEvent.onVesselUnloaded.Fire(__instance);
        }
    }
}

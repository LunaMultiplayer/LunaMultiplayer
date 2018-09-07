using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when a vessel is initialized
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("Initialize")]
    [HarmonyPatch(new[] { typeof(bool) })]
    public class Vessel_Initialize
    {
        [HarmonyPrefix]
        private static void PrefixInitialize(Vessel __instance, bool fromShipAssembly)
        {
            VesselInitializeEvent.onVesselInitializing.Fire(__instance, fromShipAssembly);
        }

        [HarmonyPostfix]
        private static void PostfixUnload(Vessel __instance, bool fromShipAssembly)
        {
            VesselInitializeEvent.onVesselInitialized.Fire(__instance, fromShipAssembly);
        }
    }
}

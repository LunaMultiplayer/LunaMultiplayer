using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
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
        private static void PostfixInitialize(Vessel __instance, bool fromShipAssembly)
        {
            VesselInitializeEvent.onVesselInitialized.Fire(__instance, fromShipAssembly);
        }
    }

    /// <summary>
    /// This harmony patch is intended to trigger an event when a vessel is initialized
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("Initialize")]
    [HarmonyPatch(new[] { typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public class Vessel_Initialize2
    {
        [HarmonyPrefix]
        private static void PrefixInitialize(Vessel __instance, bool fromShipAssembly, bool preCreate, bool orbiting, bool setActiveVessel)
        {
            VesselInitializeEvent.onVesselInitializing.Fire(__instance, fromShipAssembly);
        }

        [HarmonyPostfix]
        private static void PostfixInitialize(Vessel __instance, bool fromShipAssembly, bool preCreate, bool orbiting, bool setActiveVessel)
        {
            VesselInitializeEvent.onVesselInitialized.Fire(__instance, fromShipAssembly);
        }
    }
}

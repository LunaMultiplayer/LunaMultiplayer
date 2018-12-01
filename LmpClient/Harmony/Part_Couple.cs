using Harmony;
using LmpClient.Events;
using LmpClient.VesselUtilities;
using System;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when coupling a part
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("Couple")]
    public class Part_Couple
    {
        [HarmonyPrefix]
        private static bool PrefixCouple(Part __instance, Part tgtPart, ref Guid __state)
        {
            if (VesselCommon.IsSpectating) return false;

            __state = __instance.vessel.id;
            PartEvent.onPartCoupling.Fire(__instance, tgtPart);

            return true;
        }

        [HarmonyPostfix]
        private static void PostfixCouple(Part __instance, Part tgtPart, ref Guid __state)
        {
            if (VesselCommon.IsSpectating) return;

            PartEvent.onPartCoupled.Fire(__instance, tgtPart, __state);
        }
    }
}

using Harmony;
using LmpClient.Events;
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
        private static void PrefixCouple(Part __instance, Part tgtPart, ref Guid __state)
        {
            __state = __instance.vessel.id;
            PartEvent.onPartCoupling.Fire(__instance, tgtPart);
        }

        [HarmonyPostfix]
        private static void PostfixCouple(Part __instance, Part tgtPart, ref Guid __state)
        {
            PartEvent.onPartCoupled.Fire(__instance, tgtPart, __state);
        }
    }
}

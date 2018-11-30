using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when decoupling a part
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("decouple")]
    public class Part_Decouple
    {
        [HarmonyPrefix]
        private static void PrefixDecouple(Part __instance, float breakForce, Vessel __state)
        {
            __state = __instance.vessel;
            PartEvent.onPartDecoupling.Fire(__instance, breakForce);
        }

        [HarmonyPostfix]
        private static void PostfixDecouple(Part __instance, float breakForce, Vessel __state)
        {
            PartEvent.onPartDecoupled.Fire(__instance, breakForce, __state);
        }
    }
}

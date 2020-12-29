using HarmonyLib;
using LmpClient.Events;
using Strategies;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when a strategy has been deactivated
    /// </summary>
    [HarmonyPatch(typeof(Strategy))]
    [HarmonyPatch("Deactivate")]
    public class Strategy_Deactivate
    {
        [HarmonyPostfix]
        private static void PostfixDeactivate(Strategy __instance, ref bool __result)
        {
            if (__result)
            {
                StrategyEvent.onStrategyDeactivated.Fire(__instance);
            }
        }
    }
}

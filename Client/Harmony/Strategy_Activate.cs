using Harmony;
using LunaClient.Events;
using Strategies;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when a strategy has been activated
    /// </summary>
    [HarmonyPatch(typeof(Strategy))]
    [HarmonyPatch("Activate")]
    public class Strategy_Activate
    {
        [HarmonyPostfix]
        private static void PostfixDeactivate(Strategy __instance, ref bool __result)
        {
            if (__result)
            {
                StrategyEvent.onStrategyActivated.Fire(__instance);
            }
        }
    }
}

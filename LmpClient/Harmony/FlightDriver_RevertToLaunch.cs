using HarmonyLib;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when reverting to launch
    /// </summary>
    [HarmonyPatch(typeof(FlightDriver))]
    [HarmonyPatch("RevertToLaunch")]
    public class FlightDriver_RevertToLaunch
    {
        [HarmonyPrefix]
        private static void PrefixRevertToLaunch()
        {
            RevertEvent.onRevertingToLaunch.Fire();
        }

        [HarmonyPostfix]
        private static void PostfixRevertToLaunch()
        {
            RevertEvent.onRevertedToLaunch.Fire();
        }
    }
}

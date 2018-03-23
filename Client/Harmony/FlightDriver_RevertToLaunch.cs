using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
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
            RevertEvent.onRevertToLaunch.Fire();
        }
    }
}

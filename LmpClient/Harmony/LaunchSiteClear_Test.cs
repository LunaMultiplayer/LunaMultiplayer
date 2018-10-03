using Harmony;
using LmpCommon.Enums;
using PreFlightTests;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to fix the tests that are run before launching a vessel.
    /// When launching a vessel from editor or when clicking the runway KSC checks if there are vessels there.
    /// For LMP we just ignore that check and return true always
    /// </summary>
    [HarmonyPatch(typeof(LaunchSiteClear))]
    [HarmonyPatch("Test")]
    public class LaunchSiteClear_Test
    {
        [HarmonyPostfix]
        private static void PostfixTest(ref bool __result)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            __result = true;
        }
    }
}

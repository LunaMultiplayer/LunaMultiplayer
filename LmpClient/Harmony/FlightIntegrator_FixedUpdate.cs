using Harmony;
using LmpCommon.Enums;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the FlightIntegrator on vessels that are not ours
    /// </summary>
    [HarmonyPatch(typeof(FlightIntegrator))]
    [HarmonyPatch("FixedUpdate")]
    public class FlightIntegrator_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixFixedUpdate(FlightIntegrator __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.Vessel == FlightGlobals.ActiveVessel) return true;
            
            return __instance.Vessel.rootPart == null || !float.IsPositiveInfinity(__instance.Vessel.rootPart.crashTolerance);
        }
    }
}

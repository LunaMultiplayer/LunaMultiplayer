using Harmony;
using LmpClient.Extensions;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the FlightIntegrator on vessels that are not ours
    /// </summary>
    [HarmonyPatch(typeof(FlightIntegrator))]
    [HarmonyPatch("Update")]
    public class FlightIntegrator_Update
    {
        [HarmonyPrefix]
        private static bool PrefixUpdate(FlightIntegrator __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;
            if (__instance.Vessel == FlightGlobals.ActiveVessel) return true;

            return !__instance.Vessel.IsImmortal();
        }
    }
}

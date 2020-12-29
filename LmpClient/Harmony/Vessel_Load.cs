using HarmonyLib;
using LmpClient.Systems.SafetyBubble;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid loading a vessel if it's in safety bubble
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("Load")]
    public class Vessel_Load
    {
        [HarmonyPrefix]
        private static bool PrefixLoad(Vessel __instance)
        {
            if (FlightGlobals.ActiveVessel != null && FlightGlobals.ActiveVessel.loaded && FlightGlobals.ActiveVessel.id != __instance.id)
            {
                return !SafetyBubbleSystem.Singleton.IsInSafetyBubble(__instance);
            }

            return true;
        }
    }
}
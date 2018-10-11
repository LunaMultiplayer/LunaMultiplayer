using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when FINISHED unpacking a vessel
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("GoOffRails")]
    public class Vessel_GoOffRails
    {
        [HarmonyPostfix]
        private static void PostfixGoOffRails(Vessel __instance)
        {
            RailEvent.onVesselGoneOffRails.Fire(__instance);
        }
    }
}

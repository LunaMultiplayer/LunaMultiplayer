using Harmony;
using LmpClient.Events;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when FINISHED packing a vessel
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("GoOnRails")]
    public class Vessel_GoOnRails
    {
        [HarmonyPostfix]
        private static void PostfixGoOnRails(Vessel __instance)
        {
            RailEvent.onVesselGoneOnRails.Fire(__instance);
        }
    }
}

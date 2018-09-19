using Harmony;
using LmpClient.Events;
// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when successfully unboarding an external seat
    /// </summary>
    [HarmonyPatch(typeof(KerbalEVA))]
    [HarmonyPatch("OnDeboardSeat")]
    public class KerbalEVA_OnDeboardSeat
    {
        private static Vessel DeboardedVessel;

        [HarmonyPrefix]
        private static void PrefixOnDeboardSeat(KerbalEVA __instance)
        {
            DeboardedVessel = __instance.vessel;
        }

        [HarmonyPostfix]
        private static void PostfixOnDeboardSeat(KerbalEVA __instance)
        {
            ExternalSeatEvent.onExternalSeatUnboard.Fire(DeboardedVessel, __instance);
        }
    }
}

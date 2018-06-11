using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when successfully unboarding an external seat
    /// </summary>
    [HarmonyPatch(typeof(KerbalEVA))]
    [HarmonyPatch("OnDeboardSeat")]
    public class KerbalEVA_OnDeboardSeat
    {
        [HarmonyPostfix]
        private static void PostfixOnDeboardSeat(KerbalEVA __instance)
        {
            var unboardedSeat = Traverse.Create(typeof(KerbalEVA)).Field("kerbalSeat").GetValue<KerbalSeat>();
            ExternalSeatEvent.onExternalSeatUnboard.Fire(unboardedSeat, __instance);
        }
    }
}

using Harmony;
using LunaClient.Events;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when successfully boarding an external seat
    /// </summary>
    [HarmonyPatch(typeof(KerbalEVA))]
    [HarmonyPatch("BoardSeat")]
    public class KerbalEVA_BoardSeat
    {

        [HarmonyPostfix]
        private static void PostfixBoardSeat(KerbalEVA __instance, ref bool __result, KerbalSeat seat)
        {
            if (__result)
            {
                ExternalSeatEvent.onExternalSeatBoard.Fire(seat, __instance);
            }
        }
    }
}

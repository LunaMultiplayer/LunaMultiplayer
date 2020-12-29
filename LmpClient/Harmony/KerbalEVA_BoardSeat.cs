using Harmony;
using LmpClient.Events;
using System;
// ReSharper disable All

namespace LmpClient.ModuleStore.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when successfully boarding an external seat
    /// </summary>
    [HarmonyPatch(typeof(KerbalEVA))]
    [HarmonyPatch("BoardSeat")]
    public class KerbalEVA_BoardSeat
    {
        private static Guid KerbalVesselId;
        private static string KerbalName;

        [HarmonyPrefix]
        private static void PrefixBoardSeat(KerbalEVA __instance, KerbalSeat seat)
        {
            if (__instance.vessel != null)
            {
                KerbalVesselId = __instance.vessel.id;
                KerbalName = __instance.vessel.vesselName;
            }
        }

        [HarmonyPostfix]
        private static void PostfixBoardSeat(KerbalEVA __instance, bool __result, KerbalSeat seat)
        {
            if (__result)
            {
                ExternalSeatEvent.onExternalSeatBoard.Fire(seat.vessel, KerbalVesselId, KerbalName);
            }
        }
    }
}

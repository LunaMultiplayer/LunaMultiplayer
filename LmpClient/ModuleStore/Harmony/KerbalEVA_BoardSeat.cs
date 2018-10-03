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
        private static uint KerbalVesselPersistentId;
        private static Guid KerbalVesselId;
        private static string KerbalName;

        [HarmonyPrefix]
        private static void PrefixBoardSeat(KerbalEVA __instance, ref bool __result, KerbalSeat seat)
        {
            if (__instance.vessel != null)
            {
                KerbalVesselPersistentId = __instance.vessel.persistentId;
                KerbalVesselId = __instance.vessel.id;
                KerbalName = __instance.vessel.vesselName;
            }
        }

        [HarmonyPostfix]
        private static void PostfixBoardSeat(KerbalEVA __instance, ref bool __result, KerbalSeat seat)
        {
            if (__result)
            {
                ExternalSeatEvent.onExternalSeatBoard.Fire(seat, KerbalVesselId, KerbalVesselPersistentId, KerbalName);
            }
        }
    }
}

using Harmony;
using LmpClient.Events;
using System;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when boarding a vessel
    /// </summary>
    [HarmonyPatch(typeof(KerbalEVA))]
    [HarmonyPatch("proceedAndBoard")]
    public class KerbalEVA_proceedAndBoard
    {
        private static Guid kerbalVesselId = Guid.Empty;
        private static string kerbalName;

        [HarmonyPrefix]
        private static void PrefixProceedAndBoard(KerbalEVA __instance)
        {
            kerbalVesselId = __instance.vessel.id;
            kerbalName = __instance.vessel.name;
        }

        [HarmonyPostfix]
        private static void PostfixProceedAndBoard(Part p)
        {
            EvaEvent.onCrewEvaBoarded.Fire(kerbalVesselId, kerbalName, p.vessel);
        }
    }
}

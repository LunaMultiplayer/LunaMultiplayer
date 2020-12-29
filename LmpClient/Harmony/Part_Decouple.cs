using HarmonyLib;
using LmpClient.Events;
using LmpClient.Extensions;
using LmpClient.Systems.VesselDecoupleSys;
using LmpClient.Systems.VesselUndockSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event when decoupling a part and avoid triggering it when vessel is immortal
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("decouple")]
    public class Part_Decouple
    {
        [HarmonyPrefix]
        private static bool PrefixDecouple(Part __instance, float breakForce, ref Vessel __state)
        {
            if (MainSystem.NetworkState < ClientState.Connected || !__instance.vessel) return true;

            if (VesselDecoupleSystem.Singleton.ManuallyDecouplingVesselId == __instance.vessel.id ||
                VesselUndockSystem.Singleton.ManuallyUndockingVesselId == __instance.vessel.id ||
                !__instance.vessel.IsImmortal())
            {
                __state = __instance.vessel;
                PartEvent.onPartDecoupling.Fire(__instance, breakForce);

                return true;
            }

            return false;
        }

        [HarmonyPostfix]
        private static void PostfixDecouple(Part __instance, float breakForce, ref Vessel __state)
        {
            PartEvent.onPartDecoupled.Fire(__instance, breakForce, __state);
        }
    }
}

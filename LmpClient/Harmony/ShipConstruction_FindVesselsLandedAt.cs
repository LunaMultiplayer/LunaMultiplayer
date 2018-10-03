using System.Collections.Generic;
using Harmony;
using LmpClient.Systems.Lock;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to override the "FindVesselsLandedAt" that sometimes is called to check if there are vessels in a launch site
    /// We just remove the other controlled vessels from that check and set them correctly
    /// </summary>
    [HarmonyPatch(typeof(ShipConstruction))]
    [HarmonyPatch("FindVesselsLandedAt")]
    [HarmonyPatch(new[] { typeof(FlightState), typeof(string) })]
    public class ShipConstruction_FindVesselsLandedAt
    {
        private static readonly List<ProtoVessel> ProtoVesselsToRemove = new List<ProtoVessel>();

        [HarmonyPostfix]
        private static void PostfixFindVesselsLandedAt(List<ProtoVessel> __result)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            __result.RemoveAll(pv => LockSystem.LockQuery.ControlLockExists(pv.vesselID));
        }
    }
}

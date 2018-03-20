using Harmony;
using LunaClient.Systems.Lock;
using LunaCommon.Enums;
using System.Collections.Generic;
// ReSharper disable All

namespace LunaClient.Harmony
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
        private static readonly List<ProtoVessel> ProtoVesselsBackup = new List<ProtoVessel>();

        [HarmonyPostfix]
        private static void PostFixFindVesselsLandedAt(ref List<ProtoVessel> __result)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            __result.Clear();

            foreach (var pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                if (!LockSystem.LockQuery.ControlLockExists(pv.vesselID))
                    __result.Add(pv);
            }
        }
    }
}

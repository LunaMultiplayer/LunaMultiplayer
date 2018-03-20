using Harmony;
using LunaClient.Systems.Lock;
using LunaCommon.Enums;
using System.Collections.Generic;

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to override the "FindVesselsLandedAt" that sometimes is called to check if there are vessels in a launch site
    /// We just remove the other controlled vessels from that check and restore them afterwards
    /// </summary>
    [HarmonyPatch(typeof(ShipConstruction))]
    [HarmonyPatch("FindVesselsLandedAt")]
    [HarmonyPatch(new[] { typeof(FlightState), typeof(string) })]
    public class ShipConstructionFindVesselsLandedAtPatch
    {
        private static readonly List<ProtoVessel> ProtoVesselsBackup = new List<ProtoVessel>();

        [HarmonyPrefix]
        private static void PreFixFindVesselsLandedAt()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            ProtoVesselsBackup.Clear();

            foreach (var pv in HighLogic.CurrentGame.flightState.protoVessels)
            {
                if (LockSystem.LockQuery.ControlLockExists(pv.vesselID))
                    ProtoVesselsBackup.Add(pv);
            }

            foreach (var pv in ProtoVesselsBackup)
            {
                HighLogic.CurrentGame.flightState.protoVessels.Remove(pv);
            }
        }

        [HarmonyPostfix]
        private static void PostFixFindVesselsLandedAt()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return;

            foreach (var pv in ProtoVesselsBackup)
            {
                HighLogic.CurrentGame.flightState.protoVessels.Add(pv);
            }
        }
    }
}

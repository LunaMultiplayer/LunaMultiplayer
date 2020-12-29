using HarmonyLib;
using LmpClient.Extensions;
using LmpClient.Systems.VesselPositionSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid run the part checks in vessels that are immortal
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("_CheckPartG")]
    public class Part_CheckPartG
    {
        [HarmonyPrefix]
        private static bool PrefixCheckPartG(Part p)
        {
            if (MainSystem.NetworkState < ClientState.Connected || !p.vessel) return true;

            if (p.vessel.IsImmortal())
                return false;

            //The vessel have updates queued as it was left there by a player in a future subspace
            if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(p.vessel.id))
                return false;

            return true;
        }
    }
}

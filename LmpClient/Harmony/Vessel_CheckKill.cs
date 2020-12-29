using HarmonyLib;
using LmpClient.Extensions;
using LmpClient.Systems.VesselPositionSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid run the kill checks in vessels that are immortal
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("CheckKill")]
    public class Vessel_CheckKill
    {
        [HarmonyPrefix]
        private static bool PrefixCheckKill(Vessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected || !__instance) return true;

            if (__instance.IsImmortal())
                return false;

            //The vessel have updates queued as it was left there by a player in a future subspace
            if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(__instance.id))
                return false;

            return true;
        }
    }
}

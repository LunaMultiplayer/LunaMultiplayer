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
    [HarmonyPatch(typeof(PartBuoyancy))]
    [HarmonyPatch("FixedUpdate")]
    public class PartBuoyancy_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixCheckPartG(Part ___part)
        {
            if (MainSystem.NetworkState < ClientState.Connected || !___part || !___part.vessel) return true;

            if (___part.vessel.IsImmortal())
                return false;

            //The vessel have updates queued as it was left there by a player in a future subspace
            if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(___part.vessel.id))
                return false;

            return true;
        }
    }
}

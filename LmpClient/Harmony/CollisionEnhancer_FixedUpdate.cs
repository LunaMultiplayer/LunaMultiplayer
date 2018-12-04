using Harmony;
using LmpClient.Extensions;
using LmpClient.Systems.VesselPositionSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid run the collision checks in vessels that are immortal
    /// </summary>
    [HarmonyPatch(typeof(CollisionEnhancer))]
    [HarmonyPatch("FixedUpdate")]
    public class CollisionEnhancer_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixFixedUpdate(CollisionEnhancer __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected || !__instance.part || !__instance.part.vessel) return true;
            
            if (__instance.part.vessel.IsImmortal())
                return false;

            //The vessel have updates queued as it was left there by a player in a future subspace
            if (VesselPositionSystem.Singleton.VesselHavePositionUpdatesQueued(__instance.part.vessel.id))
                return false;

            return true;
        }
    }
}

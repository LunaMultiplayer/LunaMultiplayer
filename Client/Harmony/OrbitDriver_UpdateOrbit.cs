using Harmony;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to override the orbit updating that KSP does and rely only on LMP
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("UpdateOrbit")]
    public class OrbitDriver_UpdateOrbit
    {
        [HarmonyPrefix]
        private static bool PrefixUpdateOrbit(OrbitDriver __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected || __instance.vessel == null) return true;

            //if (LockSystem.LockQuery.ControlLockExists(__instance.vessel.id) && !LockSystem.LockQuery.ControlLockBelongsToPlayer(__instance.vessel.id, SettingsSystem.CurrentSettings.PlayerName))
            //{
            //    __instance.LmpUpdateOrbit(__instance.orbit.epoch);
            //    return false;
            //}

            return true;
        }
    }
}

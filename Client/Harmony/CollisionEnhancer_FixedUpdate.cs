using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselImmortalSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the collision checks of a vessel in case this vessel is from another player.
    /// Consider it an extension of the vesselimmortalsystem
    /// </summary>
    [HarmonyPatch(typeof(CollisionEnhancer))]
    [HarmonyPatch("FixedUpdate")]
    public class CollisionEnhancer_FixedUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixFixedUpdate(CollisionEnhancer __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!VesselImmortalSystem.Singleton.Enabled) return true;

            if (__instance?.part?.vessel != null)
            {
                if (!LockSystem.LockQuery.UpdateLockExists(__instance.part.vessel.id)) return true;
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(__instance.part.vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}

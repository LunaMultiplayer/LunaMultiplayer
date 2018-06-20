using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid run the kill checks in vessels that are not yours
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("CheckKill")]
    public class Vessel_CheckKill
    {
        [HarmonyPrefix]
        private static bool PrefixCheckKill(Vessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return !LockSystem.LockQuery.UnloadedUpdateLockExists(__instance.id) || 
                   LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(__instance.id, SettingsSystem.CurrentSettings.PlayerName);
        }
    }
}

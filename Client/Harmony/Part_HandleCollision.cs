using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
using UnityEngine;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid part explosions on other ppl vessels
    /// </summary>
    [HarmonyPatch(typeof(Part))]
    [HarmonyPatch("HandleCollision")]
    public class Part_HandleCollision
    {
        [HarmonyPrefix]
        private static bool PrefixHandleCollision(Part __instance, Collision c)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (__instance?.vessel != null)
            {
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(__instance.vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}

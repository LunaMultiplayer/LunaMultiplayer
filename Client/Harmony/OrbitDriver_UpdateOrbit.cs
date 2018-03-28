using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("UpdateOrbit")]
    public class OrbitDriver_UpdateOrbit
    {
        [HarmonyPrefix]
        private static bool PrefixUpdateOrbit(OrbitDriver __instance, bool offset)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (SettingsSystem.CurrentSettings.OverrideIntegrator && __instance?.vessel != null)
            {
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(__instance.vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}

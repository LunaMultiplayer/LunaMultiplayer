using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    [HarmonyPatch(typeof(VesselPrecalculate))]
    [HarmonyPatch("Update")]
    public class VesselPrecalculate_Update
    {
        [HarmonyPrefix]
        private static bool PrefixUpdate(VesselPrecalculate __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (SettingsSystem.CurrentSettings.OverrideIntegrator && __instance?.Vessel != null)
            {
                return LockSystem.LockQuery.UpdateLockBelongsToPlayer(__instance.Vessel.id, SettingsSystem.CurrentSettings.PlayerName);
            }

            return true;
        }
    }
}

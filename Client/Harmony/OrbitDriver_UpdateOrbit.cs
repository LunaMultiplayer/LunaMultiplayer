using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to freeze the other player vessels position if we don't have messages to interpolate to
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("UpdateOrbit")]
    public class OrbitDriver_UpdateOrbit
    {
        [HarmonyPrefix]
        private static bool PrefixUpdateOrbit(OrbitDriver __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected || __instance.vessel == null) return true;

            var vesselId = __instance.vessel.id;

            if (!LockSystem.LockQuery.UnloadedUpdateLockExists(vesselId)) return true;

            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(vesselId, SettingsSystem.CurrentSettings.PlayerName))
                return true;

            if (VesselPositionSystem.CurrentVesselUpdate.TryGetValue(vesselId, out var curPos))
            {
                if (SettingsSystem.CurrentSettings.Debug1) return false;
                return !curPos.Frozen;
            }

            return true;
        }
    }
}

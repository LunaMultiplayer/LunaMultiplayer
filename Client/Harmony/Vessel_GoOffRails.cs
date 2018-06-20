using Harmony;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselPositionSys;
using LunaCommon.Enums;

// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to avoid UNPACKING the vessel if we don't have messages to interpolate to
    /// </summary>
    [HarmonyPatch(typeof(Vessel))]
    [HarmonyPatch("GoOffRails")]
    public class Vessel_GoOffRails
    {
        [HarmonyPrefix]
        private static bool PrefixGoOffRails(Vessel __instance)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!LockSystem.LockQuery.UnloadedUpdateLockExists(__instance.id)) return true;

            if (LockSystem.LockQuery.UnloadedUpdateLockBelongsToPlayer(__instance.id, SettingsSystem.CurrentSettings.PlayerName))
                return true;

            if (VesselPositionSystem.CurrentVesselUpdate.TryGetValue(__instance.id, out var curPos))
            {
                if (SettingsSystem.CurrentSettings.Debug2) return false;
                return !curPos.Frozen;
            }

            return true;
        }
    }
}

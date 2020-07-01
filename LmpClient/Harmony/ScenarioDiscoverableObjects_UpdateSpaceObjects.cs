using Harmony;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the spawn of an asteroid or a comet if we don't have the lock or the server doesn't allow them
    /// </summary>
    [HarmonyPatch(typeof(ScenarioDiscoverableObjects))]
    [HarmonyPatch("UpdateSpaceObjects")]
    public class ScenarioDiscoverableObjects_UpdateSpaceObjects
    {
        [HarmonyPrefix]
        private static bool PrefixUpdateSpaceObjects()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!LockSystem.LockQuery.AsteroidCometLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
                return false;

            return true;
        }
    }
}

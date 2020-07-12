using Harmony;
using LmpClient.Systems.AsteroidComet;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the spawn of a comet if we don't have the lock or the server doesn't allow them
    /// </summary>
    [HarmonyPatch(typeof(ScenarioDiscoverableObjects))]
    [HarmonyPatch("SpawnComet")]
    public class ScenarioDiscoverableObjects_SpawnComet
    {
        [HarmonyPrefix]
        private static bool PrefixSpawnComet()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!LockSystem.LockQuery.AsteroidCometLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
                return false;

            var currentComets = AsteroidCometSystem.Singleton.GetCometCount();
            if (currentComets >= SettingsSystem.ServerSettings.MaxNumberOfComets)
            {
                return false;
            }

            return true;
        }
    }
}

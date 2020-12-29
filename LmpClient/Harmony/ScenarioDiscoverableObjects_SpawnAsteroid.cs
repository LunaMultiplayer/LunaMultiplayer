using HarmonyLib;
using LmpClient.Systems.AsteroidComet;
using LmpClient.Systems.Lock;
using LmpClient.Systems.SettingsSys;
using LmpCommon.Enums;

// ReSharper disable All

namespace LmpClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the spawn of an asteroid if we don't have the lock or the server doesn't allow them
    /// </summary>
    [HarmonyPatch(typeof(ScenarioDiscoverableObjects))]
    [HarmonyPatch("SpawnAsteroid")]
    public class ScenarioDiscoverableObjects_SpawnAsteroid
    {
        [HarmonyPrefix]
        private static bool PrefixSpawnAsteroid()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!LockSystem.LockQuery.AsteroidCometLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
                return false;

            var currentAsteroids = AsteroidCometSystem.Singleton.GetAsteroidCount();
            if (currentAsteroids >= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
            {
                return false;
            }

            return true;
        }
    }
}
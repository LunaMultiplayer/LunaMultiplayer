using Harmony;
using LunaClient.Systems.Asteroid;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaCommon.Enums;
// ReSharper disable All

namespace LunaClient.Harmony
{
    /// <summary>
    /// This harmony patch is intended to skip the spawn of an asteroid if we don't have the lock or the server doesn't allow them
    /// </summary>
    [HarmonyPatch(typeof(DiscoverableObjectsUtil))]
    [HarmonyPatch("SpawnAsteroid")]
    public class DiscoverableObjectsUtil_SpawnAsteroid
    {
        [HarmonyPrefix]
        private static bool PrefixSpawnAsteroid()
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            return LockSystem.LockQuery.AsteroidLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName) &&
                   AsteroidSystem.Singleton.GetAsteroidCount() < SettingsSystem.ServerSettings.MaxNumberOfAsteroids;
        }
    }
}

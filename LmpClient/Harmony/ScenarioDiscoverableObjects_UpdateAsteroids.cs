using Harmony;
using LmpClient.Systems.Asteroid;
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
    [HarmonyPatch("UpdateAsteroids")]
    public class ScenarioDiscoverableObjects_UpdateAsteroids
    {
        [HarmonyPrefix]
        private static bool PrefixUpdateAsteroids(double UT)
        {
            if (MainSystem.NetworkState < ClientState.Connected) return true;

            if (!LockSystem.LockQuery.AsteroidLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
                return false;
            
            var currentAsteroids = AsteroidSystem.Singleton.GetAsteroidCount();
            if (currentAsteroids >= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
            {
                return false;
            }

            return true;
        }
    }
}

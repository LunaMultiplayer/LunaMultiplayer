using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.VesselStore;
using System;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidEventHandler : SubSystem<AsteroidSystem>
    {
        public void OnAsteroidSpawned(Vessel asteroid)
        {
            if (LockSystem.LockQuery.AsteroidLockBelongsToPlayer(SettingsSystem.CurrentSettings.PlayerName))
            {
                if (System.GetAsteroidCount() <= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
                {
                    System.ServerAsteroids.Add(asteroid.id.ToString());
                    SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(asteroid, true);
                    VesselsProtoStore.AddVesselToDictionary(asteroid);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Killing non-server asteroid {asteroid.id}");
                    TryKillAsteroid(asteroid);
                }
            }
            else
            {
                LunaLog.Log($"[LMP]: Killing non-server asteroid {asteroid.id}, we don't own the asteroid lock");
                TryKillAsteroid(asteroid);
            }
        }

        private static void TryKillAsteroid(Vessel asteroid)
        {
            try
            {
                asteroid.Die();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
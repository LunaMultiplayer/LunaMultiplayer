using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidEventHandler : SubSystem<AsteroidSystem>
    {
        public void OnGameSceneLoadRequested(GameScenes scene)
        {
            //Force the worker to find the scenario module again.
            System.ResetAsteroidsSeed();
        }

        public void OnAsteroidSpawned(Vessel asteroid)
        {
            if (SystemsContainer.Get<LockSystem>().LockIsOurs("asteroid"))
            {
                if (System.GetAsteroidCount() <= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
                {
                    System.ServerAsteroids.Add(asteroid.id.ToString());
                    SystemsContainer.Get<VesselProtoSystem>().MessageSender.SendVesselMessage(asteroid);
                }
                else
                {
                    LunaLog.Log($"[LMP]: Killing non-server asteroid {asteroid.id}");
                    asteroid.Die();
                }
            }
            else
            {
                LunaLog.Log($"[LMP]: Killing non-server asteroid {asteroid.id}, we don't own the asteroid lock");
                asteroid.Die();
            }
        }
    }
}
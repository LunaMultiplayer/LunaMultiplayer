using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using LunaClient.Utilities;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidEventHandler : SubSystem<AsteroidSystem>
    {
        public void OnVesselCreate(Vessel checkVessel)
        {
            if (System.Enabled && System.VesselIsAsteroid(checkVessel) && !System.ServerAsteroids.Contains(checkVessel.id.ToString()))
            {
                lock (System.ServerAsteroidListLock)
                {
                    if (LockSystem.Singleton.LockIsOurs("asteroid"))
                    {
                        if (System.GetAsteroidCount() <= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
                        {
                            LunaLog.Debug("Spawned in new server asteroid!");
                            System.ServerAsteroids.Add(checkVessel.id.ToString());
                            //TODO change this
                            VesselProtoSystem.Singleton.MessageSender.SendVesselProtoMessage(checkVessel.protoVessel);
                        }
                        else
                        {
                            LunaLog.Debug("Killing non-server asteroid " + checkVessel.id);
                            checkVessel.Die();
                        }
                    }
                    else
                    {
                        LunaLog.Debug($"Killing non-server asteroid {checkVessel.id}, we don't own the asteroid lock");
                        checkVessel.Die();
                    }
                }
            }
        }

        public void OnGameSceneLoadRequested(GameScenes scene)
        {
            //Force the worker to find the scenario module again.
            System.ScenarioController = null;
        }
    }
}
using LunaClient.Base;
using LunaClient.Systems.Lock;
using LunaClient.Systems.SettingsSys;
using LunaClient.Systems.VesselProtoSys;
using UnityEngine;

namespace LunaClient.Systems.Asteroid
{
    public class AsteroidEventHandler : SubSystem<AsteroidSystem>
    {
        public void OnVesselCreate(Vessel checkVessel)
        {
            if (System.Enabled && System.VesselIsAsteroid(checkVessel) && !System.ServerAsteroids.Contains(checkVessel.id.ToString()))
            {
                if (LockSystem.Singleton.LockIsOurs("asteroid"))
                {
                    if (System.GetAsteroidCount() <= SettingsSystem.ServerSettings.MaxNumberOfAsteroids)
                    {
                        Debug.Log("[LMP]: Spawned in new server asteroid!");
                        System.ServerAsteroids.Add(checkVessel.id.ToString());
                        VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(checkVessel);
                    }
                    else
                    {
                        Debug.Log("[LMP]: Killing non-server asteroid " + checkVessel.id);
                        checkVessel.Die();
                    }
                }
                else
                {
                    Debug.Log($"[LMP]: Killing non-server asteroid {checkVessel.id}, we don't own the asteroid lock");
                    checkVessel.Die();
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
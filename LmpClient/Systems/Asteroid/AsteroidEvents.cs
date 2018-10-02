using LmpClient.Base;
using LmpClient.Systems.VesselProtoSys;
using LmpCommon.Locks;

namespace LmpClient.Systems.Asteroid
{
    public class AsteroidEvents : SubSystem<AsteroidSystem>
    {
        /// <summary>
        /// Try to get asteroid lock
        /// </summary>
        public void LockReleased(LockDefinition lockDefinition)
        {
            if (lockDefinition.Type == LockType.Asteroid)
            {
                System.TryGetAsteroidLock();
            }
        }

        /// <summary>
        /// Try to get asteroid lock when loading a level
        /// </summary>
        public void LevelLoaded(GameScenes data)
        {
            System.TryGetAsteroidLock();
        }

        public void StartTrackingAsteroid(Vessel asteroid)
        {
            LunaLog.Log($"Started to track asteroid {asteroid.id}");
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid);
        }

        public void StopTrackingAsteroid(Vessel asteroid)
        {
            LunaLog.Log($"Stopped to track asteroid {asteroid.id}");
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid);
        }
    }
}

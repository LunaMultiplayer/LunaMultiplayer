using LmpClient.Base;
using LmpClient.Extensions;
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
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid, true);
        }

        public void StopTrackingAsteroid(Vessel asteroid)
        {
            LunaLog.Log($"Stopped to track asteroid {asteroid.id}");
            VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(asteroid, true);
        }
        
        /// <summary>
        /// This event is called when accepting a recoverasset contract or when an asteroid spawns
        /// </summary>
        public void NewVesselCreated(Vessel vessel)
        {
            if (vessel.IsAsteroid())
                VesselProtoSystem.Singleton.MessageSender.SendVesselMessage(vessel, true);
        }
    }
}

using LmpClient.Base;
using LmpClient.Systems.VesselProtoSys;

namespace LmpClient.Systems.Asteroid
{
    public class AsteroidEvents : SubSystem<AsteroidSystem>
    {
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

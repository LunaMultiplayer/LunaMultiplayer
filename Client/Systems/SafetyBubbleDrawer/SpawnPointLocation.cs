using UnityEngine;

namespace LunaClient.Systems.SafetyBubbleDrawer
{
    public class SpawnPointLocation
    {
        public readonly double Latitude;
        public readonly double Longitude;
        public readonly double Altitude;
        public Transform Transform;
        public readonly CelestialBody Body;

        public Vector3d Position => Body.GetWorldSurfacePosition(Latitude, Longitude, Altitude);

        public SpawnPointLocation(PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint, CelestialBody celestialBody)
        {
            Transform = spawnPoint.GetSpawnPointTransform();
            Latitude = spawnPoint.latitude;
            Longitude = spawnPoint.longitude;
            Altitude = spawnPoint.altitude;
            Body = celestialBody;
        }

        public SpawnPointLocation(LaunchSite.SpawnPoint spawnPoint, CelestialBody celestialBody)
        {
            Transform = spawnPoint.GetSpawnPointTransform();
            Latitude = spawnPoint.latitude;
            Longitude = spawnPoint.longitude;
            Altitude = spawnPoint.altitude;
            Body = celestialBody;
        }
    }
}

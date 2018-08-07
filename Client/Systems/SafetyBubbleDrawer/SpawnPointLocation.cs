namespace LunaClient.Systems.SafetyBubbleDrawer
{
    public class SpawnPointLocation
    {
        private readonly double _latitude;
        private readonly double _longitude;
        private readonly double _altitude;
        private readonly CelestialBody _body;

        public Vector3d Position => _body.GetWorldSurfacePosition(_latitude, _longitude, _altitude);

        public SpawnPointLocation(PSystemSetup.SpaceCenterFacility.SpawnPoint spawnPoint,
            CelestialBody celestialBody)
        {
            _latitude = spawnPoint.latitude;
            _longitude = spawnPoint.longitude;
            _altitude = spawnPoint.altitude;
            _body = celestialBody;
        }

        public SpawnPointLocation(LaunchSite.SpawnPoint spawnPoint, CelestialBody celestialBody)
        {
            _latitude = spawnPoint.latitude;
            _longitude = spawnPoint.longitude;
            _altitude = spawnPoint.altitude;
            _body = celestialBody;
        }
    }
}

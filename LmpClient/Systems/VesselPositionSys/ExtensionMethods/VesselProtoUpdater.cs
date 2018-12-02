namespace LmpClient.Systems.VesselPositionSys.ExtensionMethods
{
    public static class VesselProtoUpdater
    {
        public static void UpdatePositionValues(this ProtoVessel protoVessel, Vessel vessel)
        {
            if (protoVessel == null) return;

            protoVessel.latitude = vessel.latitude;
            protoVessel.longitude = vessel.longitude;
            protoVessel.altitude = vessel.altitude;
            protoVessel.height = vessel.heightFromTerrain;

            protoVessel.normal[0] = vessel.terrainNormal.x;
            protoVessel.normal[1] = vessel.terrainNormal.y;
            protoVessel.normal[2] = vessel.terrainNormal.z;

            protoVessel.rotation[0] = vessel.srfRelRotation.x;
            protoVessel.rotation[1] = vessel.srfRelRotation.y;
            protoVessel.rotation[2] = vessel.srfRelRotation.z;
            protoVessel.rotation[3] = vessel.srfRelRotation.w;

            protoVessel.orbitSnapShot.inclination = vessel.orbit.inclination;
            protoVessel.orbitSnapShot.eccentricity = vessel.orbit.eccentricity;
            protoVessel.orbitSnapShot.semiMajorAxis = vessel.orbit.semiMajorAxis;
            protoVessel.orbitSnapShot.LAN = vessel.orbit.LAN;
            protoVessel.orbitSnapShot.argOfPeriapsis = vessel.orbit.argumentOfPeriapsis;
            protoVessel.orbitSnapShot.meanAnomalyAtEpoch = vessel.orbit.meanAnomalyAtEpoch;
            protoVessel.orbitSnapShot.epoch = vessel.orbit.epoch;
            protoVessel.orbitSnapShot.ReferenceBodyIndex = vessel.orbit.referenceBody.flightGlobalsIndex;
        }

        public static void UpdatePositionValues(this ProtoVessel protoVessel, VesselPositionUpdate update)
        {
            if (protoVessel == null) return;

            protoVessel.latitude = update.LatLonAlt[0];
            protoVessel.longitude = update.LatLonAlt[1];
            protoVessel.altitude = update.LatLonAlt[2];
            protoVessel.height = update.HeightFromTerrain;

            protoVessel.normal[0] = update.Normal[0];
            protoVessel.normal[1] = update.Normal[1];
            protoVessel.normal[2] = update.Normal[2];

            protoVessel.rotation[0] = update.SrfRelRotation[0];
            protoVessel.rotation[1] = update.SrfRelRotation[1];
            protoVessel.rotation[2] = update.SrfRelRotation[2];
            protoVessel.rotation[3] = update.SrfRelRotation[3];

            protoVessel.orbitSnapShot.inclination = update.Orbit[0];
            protoVessel.orbitSnapShot.eccentricity = update.Orbit[1];
            protoVessel.orbitSnapShot.semiMajorAxis = update.Orbit[2];
            protoVessel.orbitSnapShot.LAN = update.Orbit[3];
            protoVessel.orbitSnapShot.argOfPeriapsis = update.Orbit[4];
            protoVessel.orbitSnapShot.meanAnomalyAtEpoch = update.Orbit[5];
            protoVessel.orbitSnapShot.epoch = update.Orbit[6];
            protoVessel.orbitSnapShot.ReferenceBodyIndex = (int)update.Orbit[7];
        }
    }
}

using LunaClient.Systems.SettingsSys;

namespace LunaClient.VesselUtilities
{
    public class SafetyBubble
    {
        #region Fields and Properties

        private const double KscLaunchpadLatitude = -0.0972073656774383;
        private const double KscLaunchpadLongitude = -74.5576690686149;
        private const double KscLaunchpadAltitude = 74.5676483437419;

        private const double KscRunwayLatitude = -0.0485997166968939;
        private const double KscRunwayLongitude = -74.7244722554176;
        private const double KscRunwayAltitude = 71.2173345269402;

        private const double DesertLaunchpadLatitude = -6.56038147097707;
        private const double DesertLaunchpadLongitude = -143.950039339818;
        private const double DesertLaunchpadAltitude = 826.345226691803;

        private const double DesertRunwayLatitude = -6.59970939244927;
        private const double DesertRunwayLongitude = -144.040462582416;
        private const double DesertRunwayAltitude = 823.02435584378;

        private const double WoomerangLaunchpadLatitude = 45.2896282963322;
        private const double WoomerangLaunchpadLongitude = 136.109992206036;
        private const double WoomerangLaunchpadAltitude = 741.653213000041;

        private const double IslandRunwayLatitude = -1.52927593838872;
        private const double IslandRunwayLongitude = -71.8853164314488;
        private const double IslandRunwayAltitude = 135.12223753822;

        private static CelestialBody _homeBody;
        private static CelestialBody HomeBody => _homeBody ?? (_homeBody = FlightGlobals.Bodies.Find(b => b.isHomeWorld));

        private static Vector3d KscLaunchpadPosition => HomeBody.GetWorldSurfacePosition(KscLaunchpadLatitude, KscLaunchpadLongitude, KscLaunchpadAltitude);
        private static Vector3d KscRunwayPosition => HomeBody.GetWorldSurfacePosition(KscRunwayLatitude, KscRunwayLongitude, KscRunwayAltitude);
        private static Vector3d DesertLaunchpadPosition => HomeBody.GetWorldSurfacePosition(DesertLaunchpadLatitude, DesertLaunchpadLongitude, DesertLaunchpadAltitude);
        private static Vector3d DesertRunwayPosition => HomeBody.GetWorldSurfacePosition(DesertRunwayLatitude, DesertRunwayLongitude, DesertRunwayAltitude);
        private static Vector3d WoomerangLaunchpadPosition => HomeBody.GetWorldSurfacePosition(WoomerangLaunchpadLatitude, WoomerangLaunchpadLongitude, WoomerangLaunchpadAltitude);
        private static Vector3d IslandRunwayPosition => HomeBody.GetWorldSurfacePosition(IslandRunwayLatitude, IslandRunwayLongitude, IslandRunwayAltitude);

        #endregion

        /// <summary>
        /// Returns whether the given vessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vessel vessel, bool useLatLonAltFromProto = true)
        {
            //If not at Kerbin or past ceiling we're definitely clear
            if (vessel == null || !vessel.mainBody.isHomeWorld)
                return false;

            if (vessel.situation >= Vessel.Situations.FLYING)
                return false;

            if (useLatLonAltFromProto)
                //Use the protovessel values as the normal vessel values can be affected by the position system and the situation of the vessel
                return IsInSafetyBubble(vessel.protoVessel.latitude, vessel.protoVessel.longitude, vessel.protoVessel.altitude, vessel.mainBody);

            return IsInSafetyBubble(vessel.latitude, vessel.longitude, vessel.altitude, vessel.mainBody);
        }

        /// <summary>
        /// Returns whether the given protovessel is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(ProtoVessel protoVessel)
        {
            if (protoVessel == null)
                return true;

            if (protoVessel.orbitSnapShot != null)
                return IsInSafetyBubble(protoVessel.latitude, protoVessel.longitude, protoVessel.altitude, protoVessel.orbitSnapShot.ReferenceBodyIndex);

            return false;
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(double lat, double lon, double alt, int bodyIndex)
        {
            var body = FlightGlobals.Bodies[bodyIndex];
            if (body == null || !body.isHomeWorld)
                return false;

            return IsInSafetyBubble(FlightGlobals.Bodies[bodyIndex].GetWorldSurfacePosition(lat, lon, alt));
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(double lat, double lon, double alt, CelestialBody body)
        {
            if (body == null || !body.isHomeWorld)
                return false;

            return IsInSafetyBubble(body.GetWorldSurfacePosition(lat, lon, alt));
        }

        /// <summary>
        /// Returns whether the given position is in a starting safety bubble or not.
        /// </summary>
        public static bool IsInSafetyBubble(Vector3d position)
        {
            var kscLaunchpadDistance = Vector3d.Distance(position, KscLaunchpadPosition);
            var kscRunwayDistance = Vector3d.Distance(position, KscRunwayPosition);
            var desertLaunchpadDistance = Vector3d.Distance(position, DesertLaunchpadPosition);
            var desertRunwayDistance = Vector3d.Distance(position, DesertRunwayPosition);
            var woomerangLaunchpadDistance = Vector3d.Distance(position, WoomerangLaunchpadPosition);
            var islandRunwayDistance = Vector3d.Distance(position, IslandRunwayPosition);

            return kscLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   kscRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   desertLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   desertRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   woomerangLaunchpadDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance ||
                   islandRunwayDistance < SettingsSystem.ServerSettings.SafetyBubbleDistance;
        }
    }
}

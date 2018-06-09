using System.Globalization;

namespace LunaClient.Extensions
{
    public static class OrbitExtension
    {
        public static string PrintOrbitData(this Orbit orbit)
        {
            return $"{orbit.inclination.ToString(CultureInfo.InvariantCulture)};{orbit.eccentricity.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.semiMajorAxis.ToString(CultureInfo.InvariantCulture)};{orbit.LAN.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.argumentOfPeriapsis.ToString(CultureInfo.InvariantCulture)};{orbit.meanAnomalyAtEpoch.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.epoch.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}

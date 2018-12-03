using System.Globalization;

namespace LmpClient.Extensions
{
    public static class OrbitExtension
    {
        public static string PrintOrbitDataIndex(this Orbit orbit)
        {
            return $"INCLINATION;ECCENTRICITY;SEMIMAJORAXIS;LONGITUDEOFASCENDINGNODE;ARGUMENTOFPERIAPSIS;MEANANOMALYATEPOCH;EPOCH";
        }

        public static string PrintOrbitData(this Orbit orbit)
        {
            return $"{orbit.inclination.ToString(CultureInfo.InvariantCulture)};{orbit.eccentricity.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.semiMajorAxis.ToString(CultureInfo.InvariantCulture)};{orbit.LAN.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.argumentOfPeriapsis.ToString(CultureInfo.InvariantCulture)};{orbit.meanAnomalyAtEpoch.ToString(CultureInfo.InvariantCulture)};" +
                   $"{orbit.epoch.ToString(CultureInfo.InvariantCulture)}";
        }
    }
}

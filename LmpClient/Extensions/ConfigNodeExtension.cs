using System.Linq;

namespace LmpClient.Extensions
{
    public static class ConfigNodeExtension
    {
        /// <summary>
        /// Checks if the given config node from a protovessel has NaN orbits
        /// </summary>
        public static bool VesselHasNaNPosition(this ConfigNode vesselNode)
        {
            if (vesselNode.GetValue("landed") == "True" || vesselNode.GetValue("splashed") == "True")
            {
                if (double.TryParse(vesselNode.values.GetValue("lat"), out var latitude)
                    && (double.IsNaN(latitude) || double.IsInfinity(latitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("lon"), out var longitude)
                    && (double.IsNaN(longitude) || double.IsInfinity(longitude)))
                    return true;

                if (double.TryParse(vesselNode.values.GetValue("alt"), out var altitude)
                    && (double.IsNaN(altitude) || double.IsInfinity(altitude)))
                    return true;
            }
            else
            {
                var orbitNode = vesselNode.GetNode("ORBIT");
                if (orbitNode != null)
                {
                    var allValuesAre0 = orbitNode.values.DistinctNames().Select(v => orbitNode.GetValue(v)).Take(7)
                        .All(v => v == "0");

                    return allValuesAre0 || orbitNode.values.DistinctNames().Select(v => orbitNode.GetValue(v))
                               .Any(val => double.TryParse(val, out var value) && (double.IsNaN(value) || double.IsInfinity(value)));
                }
            }

            return false;
        }
    }
}

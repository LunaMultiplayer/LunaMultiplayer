using System;
using System.Globalization;

namespace LunaClient.Utilities
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns only 2 decimals in case a module value has several to avoid reloading the part.
        /// Sometimes we get a proto part module value of 17.0001 and the part value is 17.0 so it's useless to reload
        /// a whole part module for such a small change!
        /// </summary>
        public static string FormatModuleValue(this string str)
        {
            return decimal.TryParse(str, out var decimalValue) ? 
                Math.Round(decimalValue, 2).ToString(CultureInfo.InvariantCulture) : str;
        }

        /// <summary>
        /// Prints an object as invariant culture if possible
        /// </summary>
        public static string ToInvariantString(this object obj)
        {
            return obj is IConvertible convertible ? convertible.ToString(CultureInfo.InvariantCulture)
                : (obj as IFormattable)?.ToString(null, CultureInfo.InvariantCulture) ?? obj.ToString();
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

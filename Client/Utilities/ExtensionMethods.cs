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
    }
}

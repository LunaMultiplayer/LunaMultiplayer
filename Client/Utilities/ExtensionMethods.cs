using System;
using System.Globalization;

namespace LunaClient.Utilities
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns only 2 decimals in case a module value has several to avoid reloading the part.
        /// </summary>
        public static string FormatModuleValue(this string str)
        {
            return decimal.TryParse(str, out var decimalValue) ? 
                Math.Round(decimalValue, 2).ToString(CultureInfo.InvariantCulture) : str;
        }
    }
}

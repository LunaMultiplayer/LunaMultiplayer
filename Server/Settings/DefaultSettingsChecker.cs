using Server.Log;
using Server.Settings.Definition;
using Server.Settings.Structures;
using System;
using System.Collections.Generic;

namespace Server.Settings
{
    /// <summary>
    /// Checks the loaded server settings against the built-in defaults defined in
    /// <see cref="GeneralSettingsDefinition"/> and logs a warning for any value that
    /// is still on its default. This nudges server owners to personalize the public
    /// facing fields so their server is easier to find and identify in the server list.
    /// </summary>
    public static class DefaultSettingsChecker
    {
        public static void WarnIfUsingDefaults()
        {
            var defaults = new GeneralSettingsDefinition();
            var current = GeneralSettings.SettingsStore;

            var checks = new List<(string Current, string Default, string Warning)>
            {
                (current.ServerName, defaults.ServerName,
                    "Default server name detected! Consider changing it in GeneralSettings.xml."),
                (current.Description, defaults.Description,
                    "Default server description detected! Consider changing it in GeneralSettings.xml."),
            };

            foreach (var (currentValue, defaultValue, warning) in checks)
            {
                if (string.Equals(currentValue, defaultValue, StringComparison.Ordinal))
                    LunaLog.Warning(warning);
            }
        }
    }
}

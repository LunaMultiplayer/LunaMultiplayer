using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerLogSettings
    {
        public string LogLevel => LogSettings.SettingsStore.LogLevel.ToString();
        public int ExpireLogs => LogSettings.SettingsStore.ExpireLogs;
        public bool UseUtcTimeInLog => LogSettings.SettingsStore.UseUtcTimeInLog;
    }
}

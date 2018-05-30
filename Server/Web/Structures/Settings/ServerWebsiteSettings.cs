using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerWebsiteSettings
    {
        public bool EnableWebsite => WebsiteSettings.SettingsStore.EnableWebsite;
        public int Port => WebsiteSettings.SettingsStore.Port;
        public int RefreshIntervalMs => WebsiteSettings.SettingsStore.RefreshIntervalMs;
    }
}

using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerScreenshotSettings
    {
        public int MinScreenshotIntervalMs => ScreenshotSettings.SettingsStore.MinScreenshotIntervalMs;
        public int MaxScreenshotWidth => ScreenshotSettings.SettingsStore.MaxScreenshotWidth;
        public int MaxScreenshotHeight => ScreenshotSettings.SettingsStore.MaxScreenshotHeight;
        public int MaxScreenshotsPerUser => ScreenshotSettings.SettingsStore.MaxScreenshotsPerUser;
        public int MaxScreenshotsFolders => ScreenshotSettings.SettingsStore.MaxScreenshotsFolders;
    }
}

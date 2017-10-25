using LunaServer.Context;
using System.IO;
using LunaServer.Settings.Definition;

namespace LunaServer.Settings
{
    public class GeneralSettings: SettingsBase
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "Settings.txt");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as SettingsDefinition;
        }

        public static SettingsDefinition SettingsStore { get; private set; }

        public static GeneralSettings Singleton { get; } = new GeneralSettings();
    }
}
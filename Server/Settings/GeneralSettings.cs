using Server.Context;
using Server.Settings.Definition;
using System.IO;

namespace Server.Settings
{
    public class GeneralSettings: SettingsBase<SettingsDefinition>
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
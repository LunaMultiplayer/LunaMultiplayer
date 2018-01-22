using Server.Context;
using Server.Settings.Definition;
using System.IO;

namespace Server.Settings
{
    public class DebugSettings : SettingsBase<DebugSettingsDefinition>
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "DebugSettings.txt");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as DebugSettingsDefinition;
        }

        public static DebugSettingsDefinition SettingsStore { get; private set; }

        public static DebugSettings Singleton { get; } = new DebugSettings();
    }
}

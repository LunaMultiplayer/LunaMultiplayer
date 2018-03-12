using Server.Context;
using Server.Settings.Definition;
using System;
using System.IO;

namespace Server.Settings
{
    public class DebugSettings : SettingsBase
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "DebugSettings.xml");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as DebugSettingsDefinition;
        }

        protected override Type SettingsHolderType => typeof(DebugSettingsDefinition);

        public static DebugSettingsDefinition SettingsStore { get; private set; }

        public static DebugSettings Singleton { get; } = new DebugSettings();
    }
}

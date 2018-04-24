using Server.Context;
using Server.Settings.Definition;
using System;
using System.IO;

namespace Server.Settings
{
    public class MasterServerSettings : SettingsBase
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "MasterServerSettings.xml");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as MasterServerSettingsDefinition;
        }

        protected override Type SettingsHolderType => typeof(MasterServerSettingsDefinition);

        public static MasterServerSettingsDefinition SettingsStore { get; private set; } = new MasterServerSettingsDefinition();

        public static MasterServerSettingsDefinition Singleton { get; } = new MasterServerSettingsDefinition();
    }
}

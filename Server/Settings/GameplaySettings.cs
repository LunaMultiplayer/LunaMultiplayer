using Server.Context;
using Server.Settings.Definition;
using System;
using System.IO;

namespace Server.Settings
{
    public class GameplaySettings : SettingsBase
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "GameplaySettings.xml");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as GameplaySettingsDefinition;
        }

        protected override Type SettingsHolderType => typeof(GameplaySettingsDefinition);

        public static GameplaySettingsDefinition SettingsStore { get; private set; } = new GameplaySettingsDefinition();

        public static GameplaySettings Singleton { get; } = new GameplaySettings();
    }
}
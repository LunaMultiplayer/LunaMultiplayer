using Server.Context;
using Server.Settings.Definition;
using System.IO;

namespace Server.Settings
{
    public class GameplaySettings : SettingsBase<GameplaySettingsDefinition>
    {
        protected override string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, "GameplaySettings.txt");

        protected override object SettingsHolder
        {
            get => SettingsStore;
            set => SettingsStore = value as GameplaySettingsDefinition;
        }

        public static GameplaySettingsDefinition SettingsStore { get; private set; }

        public static GameplaySettings Singleton { get; } = new GameplaySettings();
    }
}
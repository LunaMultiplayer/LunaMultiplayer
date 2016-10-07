using System.IO;
using LunaServer.Context;
using SettingsParser;

namespace LunaServer.Settings
{
    public class GameplaySettings
    {
        private static ConfigParser<GameplaySettingsStore> _gameplaySettings;

        public static GameplaySettingsStore SettingsStore => _gameplaySettings?.Settings;

        public static void Reset()
        {
            _gameplaySettings = new ConfigParser<GameplaySettingsStore>(new GameplaySettingsStore(),
                Path.Combine(ServerContext.ConfigDirectory, "GameplaySettings.txt"));
        }

        public static void Load()
        {
            _gameplaySettings.LoadSettings();
        }

        public static void Save()
        {
            _gameplaySettings.SaveSettings();
        }
    }
}
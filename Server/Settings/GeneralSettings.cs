using LunaServer.Context;
using LunaServer.System;
using LunaServer.Utilities;
using System.IO;

namespace LunaServer.Settings
{
    public class GeneralSettings
    {
        private static ConfigParser<SettingsStore> _serverSettings;
        public static SettingsStore SettingsStore => _serverSettings.Settings;

        static GeneralSettings()
        {
            if (!FileHandler.FolderExists(ServerContext.ConfigDirectory))
                FileHandler.FolderCreate(ServerContext.ConfigDirectory);

            Reset();
            Load();
        }

        public static void Reset()
        {
            _serverSettings = new ConfigParser<SettingsStore>(new SettingsStore(), Path.Combine(ServerContext.ConfigDirectory, "Settings.txt"));
        }

        public static void Load()
        {
            _serverSettings.LoadSettings();
        }

        public static void Save()
        {
            _serverSettings.SaveSettings();
        }
    }
}
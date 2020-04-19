using LmpCommon.Enums;
using Server.Log;
using Server.Settings.Base;
using Server.Settings.Definition;
using Server.Settings.Structures;
using System;
using System.Linq;
using System.Reflection;

namespace Server.Settings
{
    public static class SettingsHandler
    {
        public static void LoadSettings()
        {
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(t => typeof(ISettings).IsAssignableFrom(t) && !t.IsAbstract))
            {
                if (type == typeof(GameplaySettings))
                    continue;

                var instance = Activator.CreateInstance(type);
                ((ISettings)instance).Load();
            }

            //Load the gameplay settings last so we have the GeneralSettings.SettingsStore.GameDifficulty defined
            new GameplaySettings().Load();
        }

        public static void ValidateDifficultySettings()
        {
            if (GeneralSettings.SettingsStore.GameDifficulty == GameDifficulty.Custom)
                return;

            if (GeneralSettings.SettingsStore.GameDifficulty != GameDifficulty.Custom &&
                HasDifferencesAgainstGivenSetting(GeneralSettings.SettingsStore.GameDifficulty))
            {
                LunaLog.Info("Your GameplaySettings file is different than your GeneralSettings - Difficulty value. " +
                                "So the difficulty setting will be set as \"Custom\". " +
                                $"In case you want to use the default setting values for {GeneralSettings.SettingsStore.GameDifficulty}. " +
                                $"Remove the GameplaySettings.xml file so it's recreated again");

                GeneralSettings.SettingsStore.GameDifficulty = GameDifficulty.Custom;
            }
        }

        private static bool HasDifferencesAgainstGivenSetting(GameDifficulty difficulty)
        {
            var definition = new GameplaySettingsDefinition();
            switch (difficulty)
            {
                case GameDifficulty.Easy:
                    definition.SetEasy();
                    break;
                case GameDifficulty.Normal:
                    definition.SetNormal();
                    break;
                case GameDifficulty.Moderate:
                    definition.SetModerate();
                    break;
                case GameDifficulty.Hard:
                    definition.SetHard();
                    break;
            }

            return typeof(GameplaySettingsDefinition).GetProperties()
                .Select(p => new
                {
                    value = p.GetValue(GameplaySettings.SettingsStore).ToString(),
                    defaultVal = p.GetValue(definition).ToString()
                })
                .Any(v => v.value != v.defaultVal);
        }
    }
}

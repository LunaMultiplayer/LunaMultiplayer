using LunaClient.Localization.Structures;
using LunaClient.Utilities;
using LunaCommon.Xml;
using System;
using System.IO;
using System.Linq;

namespace LunaClient.Localization
{
    public static class LocalizationContainer
    {
        public static Languages CurrentLanguage { get; set; } = Languages.English;

        public static ConnectionWindowText ConnectionWindowText = new ConnectionWindowText();
        public static SettingsWindowText SettingsWindowText = new SettingsWindowText();

        private static readonly string LocalizationFolder = CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer", "Localization");

        #region Loading

        public static Languages GetNextLanguage()
        {
            var languages = Enum.GetValues(typeof(Languages)).Cast<Languages>().ToArray();
            for (var i = 0; i < languages.Length; i++)
            {
                if (CurrentLanguage == languages[i])
                {
                    return i + 1 == languages.Length ? 0 : (Languages)i + 1;
                }
            }

            return 0;
        }

        public static void LoadLanguage(Languages language)
        {
            CurrentLanguage = language;

            if (!Directory.Exists(LocalizationFolder))
            {
                Directory.CreateDirectory(LocalizationFolder);
            }

            LoadWindowTexts(language, ref ConnectionWindowText);
            LoadWindowTexts(language, ref SettingsWindowText);
        }

        private static void LoadWindowTexts<T>(object lang, ref T classToReplace) where T : class, new()
        {
            var filePath = CommonUtil.CombinePaths(LocalizationFolder, $"{classToReplace.GetType().Name}_{lang}.xml");
            if (!File.Exists(filePath))
                LunaXmlSerializer.WriteXml(new T(), filePath);

            classToReplace = LunaXmlSerializer.ReadXml<T>(filePath);
        }

        #endregion
    }
}

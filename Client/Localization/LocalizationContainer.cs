using LunaClient.Localization.Structures;
using LunaClient.Utilities;
using LunaCommon.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LunaClient.Localization
{
    public static class LocalizationContainer
    {
        public static readonly List<string> Languages = new List<string>();
        public static string CurrentLanguage { get; set; } = "English";

        public static AdminWindowText AdminWindowText = new AdminWindowText();
        public static BannedPartsWindowText BannedPartsWindowText = new BannedPartsWindowText();
        public static ChatWindowText ChatWindowText = new ChatWindowText();
        public static ConnectionWindowText ConnectionWindowText = new ConnectionWindowText();
        public static CraftLibraryWindowText CraftLibraryWindowText = new CraftLibraryWindowText();
        public static ModWindowText ModWindowText = new ModWindowText();
        public static OptionsWindowText OptionsWindowText = new OptionsWindowText();
        public static ServerListWindowText ServerListWindowText = new ServerListWindowText();
        public static StatusWindowText StatusWindowText = new StatusWindowText();
        public static DisclaimerDialogText DisclaimerDialogText = new DisclaimerDialogText();
        public static InstallDialogText InstallDialogText = new InstallDialogText();
        public static ScreenshotWindowText ScreenshotWindowText = new ScreenshotWindowText();
        public static ScreenText ScreenText = new ScreenText();
        public static ButtonTooltips ButtonTooltips = new ButtonTooltips();
        public static UpdateWindowText UpdateWindowText = new UpdateWindowText();
        public static CompatibleDialogText CompatibleDialogText = new CompatibleDialogText();

        private static readonly string LocalizationFolder = CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer", "Localization");

        public static string GetCurrentLanguageAsText()
        {
            return CurrentLanguage.Replace("_", " ");
        }

        #region Loading

        public static void LoadLanguages()
        {
            Languages.Clear();
            Languages.AddRange(Directory.GetDirectories(LocalizationFolder).Select(d=> new DirectoryInfo(d).Name));
        }

        public static string GetNextLanguage()
        {
            for (var i = 0; i < Languages.Count; i++)
            {
                if (CurrentLanguage == Languages[i])
                {
                    return i + 1 == Languages.Count ? Languages[0] : Languages[i + 1];
                }
            }

            return Languages[0];
        }

        public static void LoadLanguage(string language)
        {
            CurrentLanguage = language;

            if (!Directory.Exists(LocalizationFolder))
            {
                Directory.CreateDirectory(LocalizationFolder);
            }

            if (!Directory.Exists(CommonUtil.CombinePaths(LocalizationFolder, language)))
            {
                Directory.CreateDirectory(CommonUtil.CombinePaths(LocalizationFolder, language));
            }

            LoadWindowTexts(language, ref AdminWindowText);
            LoadWindowTexts(language, ref BannedPartsWindowText);
            LoadWindowTexts(language, ref ChatWindowText);
            LoadWindowTexts(language, ref ConnectionWindowText);
            LoadWindowTexts(language, ref CraftLibraryWindowText);
            LoadWindowTexts(language, ref ModWindowText);
            LoadWindowTexts(language, ref OptionsWindowText);
            LoadWindowTexts(language, ref ServerListWindowText);
            LoadWindowTexts(language, ref StatusWindowText);
            LoadWindowTexts(language, ref DisclaimerDialogText);
            LoadWindowTexts(language, ref InstallDialogText);
            LoadWindowTexts(language, ref ScreenshotWindowText);
            LoadWindowTexts(language, ref ScreenText);
            LoadWindowTexts(language, ref ButtonTooltips);
            LoadWindowTexts(language, ref UpdateWindowText);
            LoadWindowTexts(language, ref CompatibleDialogText);
        }

        private static void LoadWindowTexts<T>(string language, ref T classToReplace) where T : class, new()
        {
            try
            {
                var filePath = CommonUtil.CombinePaths(LocalizationFolder, language, $"{classToReplace.GetType().Name}.xml");
                if (!File.Exists(filePath))
                    LunaXmlSerializer.WriteToXmlFile(new T(), filePath);

                classToReplace = LunaXmlSerializer.ReadXmlFromPath<T>(filePath);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"Error reading '{classToReplace.GetType().Name}.xml' for language '{language}' Details: {e}");
            }
        }

        #endregion
    }
}

using LunaClient.Utilities;
using LunaCommon.Xml;
using System.IO;

namespace LunaClient.Systems.SettingsSys
{
    public static class SettingsReadSaveHandler
    {
        #region Path properties

        private static string DataFolderPath => CommonUtil.CombinePaths(MainSystem.KspPath, "GameData", "LunaMultiplayer",
                "Plugins", "Data");

        private static string SettingsFilePath => CommonUtil.CombinePaths(DataFolderPath, SettingsFileName);
        private static string BackupSettingsFilePath => CommonUtil.CombinePaths(DataFolderPath, BackupSettingsFileName);

        private const string SettingsFileName = "settings.xml";
        private const string BackupSettingsFileName = "settings_bkp.xml";

        #endregion

        #region Public

        public static SettingStructure ReadSettings()
        {
            CheckDataDirectory();

            RestoreBackupIfNoSettings();

            if (!File.Exists(SettingsFilePath))
                CreateDefaultSettingsFile();

            if (!File.Exists(BackupSettingsFilePath))
            {
                LunaLog.Log("[LMP]: Backing up player token and settings file!");
                File.Copy(SettingsFilePath, BackupSettingsFilePath);
            }

            return LunaXmlSerializer.ReadXmlFromPath<SettingStructure>(SettingsFilePath);
        }

        public static void SaveSettings(SettingStructure currentSettings)
        {
            CheckDataDirectory();
            LunaXmlSerializer.WriteToXmlFile(currentSettings, SettingsFilePath);
            File.Copy(SettingsFilePath, BackupSettingsFilePath, true);
        }

        #endregion

        #region Private methods

        private static void CheckDataDirectory()
        {
            if (!Directory.Exists(DataFolderPath))
                Directory.CreateDirectory(DataFolderPath);
        }

        private static void CreateDefaultSettingsFile()
        {
            var defaultSettings = new SettingStructure();
            LunaXmlSerializer.WriteToXmlFile(defaultSettings, SettingsFilePath);
        }
        
        private static void RestoreBackupIfNoSettings()
        {
            if (File.Exists(BackupSettingsFilePath) && !File.Exists(SettingsFilePath))
            {
                LunaLog.Log("[LMP]: Restoring player settings file!");
                File.Copy(BackupSettingsFilePath, SettingsFilePath);
            }
        }
        
        #endregion
    }
}

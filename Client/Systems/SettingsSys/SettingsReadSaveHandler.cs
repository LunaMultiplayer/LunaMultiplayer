using LunaClient.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;

namespace LunaClient.Systems.SettingsSys
{
    public static class SettingsReadSaveHandler
    {
        #region Path properties

        private static string DataFolderPath => CommonUtil.CombinePaths(Client.KspPath, "GameData", "LunaMultiPlayer",
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

            try
            {
                using (TextReader r = new StreamReader(SettingsFilePath))
                {
                    var deserializer = new XmlSerializer(typeof(SettingStructure));
                    var structure = (SettingStructure)deserializer.Deserialize(r);
                    return structure;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Could not open and read settings file. Details: {e}");
            }
        }

        public static void SaveSettings(SettingStructure currentSettings)
        {
            CheckDataDirectory();
            SaveSettingsToFile(currentSettings);
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

            var newKey = GenerateNewKeypair();
            defaultSettings.PrivateKey = newKey.Key;
            defaultSettings.PublicKey = newKey.Value;

            SaveSettingsToFile(defaultSettings);
        }

        private static KeyValuePair<string, string> GenerateNewKeypair()
        {
            using (var rsa = new RSACryptoServiceProvider(1024))
            {
                try
                {
                    var privateKey = rsa.ToXmlString(true);
                    var publicKey = rsa.ToXmlString(false);
                    return new KeyValuePair<string, string>(privateKey, publicKey);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error generating RSA key: {e}");
                }
                finally
                {
                    //Don't save the key in the machine store.
                    rsa.PersistKeyInCsp = false;
                }
            }
            return new KeyValuePair<string, string>("", "");
        }

        private static void RestoreBackupIfNoSettings()
        {
            if (File.Exists(BackupSettingsFilePath) && !File.Exists(SettingsFilePath))
            {
                LunaLog.Log("[LMP]: Restoring player settings file!");
                File.Copy(BackupSettingsFilePath, SettingsFilePath);
            }
        }

        private static void SaveSettingsToFile(SettingStructure settings)
        {
            try
            {
                using (var w = new XmlTextWriter(SettingsFilePath, null))
                {
                    w.Formatting = Formatting.Indented;
                    var serializer = new XmlSerializer(typeof(SettingStructure));
                    serializer.Serialize(w, settings);
                }
            }
            catch (Exception)
            {
                throw new Exception("Could not save settings file.");
            }
        }

        #endregion
    }
}
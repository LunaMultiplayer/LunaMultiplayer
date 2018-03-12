using LunaCommon.Xml;
using Server.Context;
using Server.Log;
using Server.System;
using System;
using System.IO;

namespace Server.Settings
{
    public abstract class SettingsBase
    {
        protected abstract string SettingsPath { get; }

        protected abstract object SettingsHolder { get; set; }

        protected abstract Type SettingsHolderType { get; }

        protected SettingsBase()
        {
            if (!FileHandler.FolderExists(ServerContext.ConfigDirectory))
                FileHandler.FolderCreate(ServerContext.ConfigDirectory);
        }

        public void Load()
        {
            if (!File.Exists(SettingsPath))
                LunaXmlSerializer.WriteToXmlFile(Activator.CreateInstance(SettingsHolderType), SettingsPath);

            try
            {
                SettingsHolder = LunaXmlSerializer.ReadXmlFromPath(SettingsHolderType, SettingsPath);
            }
            catch (Exception)
            {
                LunaLog.Fatal($"Error while trying to read {SettingsPath}. Default settings will be used. Please remove the file so a new one can be generated");
            }
        }

        public void Save()
        {
            LunaXmlSerializer.WriteToXmlFile(SettingsHolder, SettingsPath);
        }
    }
}

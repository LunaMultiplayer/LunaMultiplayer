using LunaCommon.Xml;
using Server.Context;
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

            Load();
        }

        public void Load()
        {
            if (!File.Exists(SettingsPath))
                LunaXmlSerializer.WriteToXmlFile(Activator.CreateInstance(SettingsHolderType), SettingsPath);

            SettingsHolder = LunaXmlSerializer.ReadXmlFromPath(SettingsHolderType, SettingsPath);
        }

        public void Save()
        {
            LunaXmlSerializer.WriteToXmlFile(SettingsHolder, SettingsPath);
        }
    }
}

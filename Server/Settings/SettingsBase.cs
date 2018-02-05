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
                LunaXmlSerializer.WriteXml(Activator.CreateInstance(SettingsHolderType), SettingsPath);

            SettingsHolder = LunaXmlSerializer.ReadXml(SettingsHolderType, SettingsPath);
        }

        public void Save()
        {
            LunaXmlSerializer.WriteXml(SettingsHolder, SettingsPath);
        }
    }
}

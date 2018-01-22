using LunaCommon.Xml;
using Server.Context;
using Server.System;
using System.IO;

namespace Server.Settings
{
    public abstract class SettingsBase<T> where T : class, new()
    {
        protected abstract string SettingsPath { get; }

        protected abstract object SettingsHolder { get; set; }
        
        protected SettingsBase()
        {
            if (!FileHandler.FolderExists(ServerContext.ConfigDirectory))
                FileHandler.FolderCreate(ServerContext.ConfigDirectory);

            Load();
        }

        public void Load()
        {
            if (!File.Exists(SettingsPath))
                LunaXmlSerializer.WriteXml(new T(), SettingsPath);

            SettingsHolder = LunaXmlSerializer.ReadXml<T>(SettingsPath);
        }

        public void Save()
        {
            LunaXmlSerializer.WriteXml(SettingsHolder, SettingsPath);
        }
    }
}

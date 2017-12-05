using System.IO;
using LunaCommon.Xml;
using Server.Context;
using Server.Settings.Definition;
using Server.System;

namespace Server.Settings
{
    public abstract class SettingsBase
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
                LunaXmlSerializer.WriteXml(new SettingsDefinition(), SettingsPath);

            SettingsHolder = LunaXmlSerializer.ReadXml<SettingsDefinition>(SettingsPath);
        }

        public void Save()
        {
            LunaXmlSerializer.WriteXml(SettingsHolder, SettingsPath);
        }
    }
}

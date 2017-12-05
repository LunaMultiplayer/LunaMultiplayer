using LMP.Server.Context;
using LMP.Server.Settings.Definition;
using LMP.Server.System;
using LunaCommon.Xml;
using System.IO;

namespace LMP.Server.Settings
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

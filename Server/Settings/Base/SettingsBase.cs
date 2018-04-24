using LunaCommon.Xml;
using Server.Context;
using Server.Log;
using Server.System;
using System;
using System.IO;

namespace Server.Settings.Base
{
    public abstract class SettingsBase<T>: ISettings
        where T : class, new()
    {
        protected abstract string Filename { get; }
        private string SettingsPath => Path.Combine(ServerContext.ConfigDirectory, Filename);
        public static T SettingsStore { get; private set; } = new T();

        protected SettingsBase()
        {
            if (!FileHandler.FolderExists(ServerContext.ConfigDirectory))
                FileHandler.FolderCreate(ServerContext.ConfigDirectory);
        }

        public void Load()
        {
            if (!File.Exists(SettingsPath))
                LunaXmlSerializer.WriteToXmlFile(Activator.CreateInstance(typeof(T)), Path.Combine(ServerContext.ConfigDirectory, Filename));

            try
            {
                SettingsStore = LunaXmlSerializer.ReadXmlFromPath(typeof(T), SettingsPath) as T;
            }
            catch (Exception)
            {
                LunaLog.Fatal($"Error while trying to read {SettingsPath}. Default settings will be used. Please remove the file so a new one can be generated");
            }
        }

        public void Save()
        {
            LunaXmlSerializer.WriteToXmlFile(SettingsStore, SettingsPath);
        }
    }
}

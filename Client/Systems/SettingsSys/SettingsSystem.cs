using LunaClient.Base;

namespace LunaClient.Systems.SettingsSys
{
    public class SettingsSystem : MessageSystem<SettingsSystem, SettingsMessageSender, SettingsMessageHandler>
    {
        public static SettingStructure CurrentSettings { get; }
        public static SettingsServerStructure ServerSettings { get; private set; } = new SettingsServerStructure();

        static SettingsSystem()
        {
            CurrentSettings = SettingsReadSaveHandler.ReadSettings();
        }

        public void SaveSettings()
        {
            SettingsReadSaveHandler.SaveSettings(CurrentSettings);
        }

        public void ResetServerSettings()
        {
            ServerSettings = new SettingsServerStructure();
        }
    }
}
using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerMasterServerSettings
    {
        public bool RegisterWithMasterServer => MasterServerSettings.SettingsStore.RegisterWithMasterServer;
        public int MasterServerRegistrationMsInterval => MasterServerSettings.SettingsStore.MasterServerRegistrationMsInterval;
    }
}

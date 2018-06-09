using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerConnectionSettings
    {
        public int Port => ConnectionSettings.SettingsStore.Port;
        public int HearbeatMsInterval => ConnectionSettings.SettingsStore.HearbeatMsInterval;
        public int ConnectionMsTimeout => ConnectionSettings.SettingsStore.ConnectionMsTimeout;
        public bool Upnp => ConnectionSettings.SettingsStore.Upnp;
        public int UpnpMsTimeout => ConnectionSettings.SettingsStore.UpnpMsTimeout;
        public int MaximumTransmissionUnit => ConnectionSettings.SettingsStore.MaximumTransmissionUnit;
        public bool AutoExpandMtu => ConnectionSettings.SettingsStore.AutoExpandMtu;
    }
}

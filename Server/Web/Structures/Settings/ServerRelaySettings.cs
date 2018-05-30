using Server.Settings.Structures;

namespace Server.Web.Structures.Settings
{
    public class ServerRelaySettings
    {
        public string RelaySystemMode => RelaySettings.SettingsStore.RelaySystemMode.ToString();
        public int RelaySaveIntervalMs => RelaySettings.SettingsStore.RelaySaveIntervalMs;
    }
}

using Server.Web.Structures.Settings;

namespace Server.Web.Structures
{
    public class ServerInformation
    {
        public CurrentState CurrentState { get; } = new CurrentState();
        public ServerGeneralSettings GeneralSettings { get; } = new ServerGeneralSettings();
        public ServerWarpSettings WarpSettings { get; } = new ServerWarpSettings();
        public ServerScreenshotSettings ServerScreenshotSettings { get; } = new ServerScreenshotSettings();
        public ServerRelaySettings RelaySettings { get; } = new ServerRelaySettings();
        public ServerMasterServerSettings ServerMasterServerSettings { get; } = new ServerMasterServerSettings();
        public ServerLogSettings ServerLogSettings { get; } = new ServerLogSettings();
        public ServerIntervalSettings ServerIntervalSettings { get; } = new ServerIntervalSettings();
        public ServerGameplaySettings ServerGameplaySettings { get; } = new ServerGameplaySettings();
        public ServerDebugSettings ServerDebugSettings { get; } = new ServerDebugSettings();
        public ServerCraftSettings ServerCraftSettings { get; } = new ServerCraftSettings();
        public ServerConnectionSettings ServerConnectionSettings { get; } = new ServerConnectionSettings();

        public void Refresh()
        {
            CurrentState.Refresh();
        }
    }
}

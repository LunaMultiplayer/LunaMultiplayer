using System.Net;

namespace LmpCommon
{
    public class ServerInfo
    {
        public long Id { get; set; }
        public string Country { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }
        public IPEndPoint InternalEndpoint6 { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public string ServerVersion { get; set; }
        public string DisplayedPing { get; set; } = "?";
        public string DisplayedPing6 { get; set; } = "?";
        public int Ping { get; set; } = int.MaxValue;
        public int Ping6 { get; set; } = int.MaxValue;
        public bool Password { get; set; }
        public bool Cheats { get; set; }
        public int GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public bool ModControl { get; set; }
        public bool DedicatedServer { get; set; }
        public int PlayerCount { get; set; }
        public string ServerName { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string WebsiteText { get; set; }
        public int WarpMode { get; set; }
        public int TerrainQuality { get; set; }
        public int VesselUpdatesSendMsInterval { get; set; }
        public int SecondaryVesselUpdatesSendMsInterval { get; set; }
        public bool RainbowEffect { get; set; }
        public byte[] Color { get; } = new byte[3];
    }
}

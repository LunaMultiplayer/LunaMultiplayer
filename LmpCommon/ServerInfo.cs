namespace LmpCommon
{
    public class ServerInfo
    {
        public long Id { get; set; }
        public string Country { get; set; }
        public string InternalEndpoint { get; set; }
        public string ExternalEndpoint { get; set; }
        public string ServerVersion { get; set; }
        public string DisplayedPing { get; set; } = "?";
        public int Ping { get; set; } = int.MaxValue;
        public bool Password { get; set; }
        public bool Cheats { get; set; }
        public int GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public bool ModControl { get; set; }
        public int PlayerCount { get; set; }
        public string ServerName { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string WebsiteText { get; set; }
        public int WarpMode { get; set; }
        public int TerrainQuality { get; set; }
        public int VesselUpdatesSendMsInterval { get; set; }
        public int SecondaryVesselUpdatesSendMsInterval { get; set; }
        public bool DropControlOnVesselSwitching { get; set; }
        public bool DropControlOnExitFlight { get; set; }
        public bool DropControlOnExit { get; set; }
    }
}

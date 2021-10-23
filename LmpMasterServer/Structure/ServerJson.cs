using System.Net;

namespace LmpCommon
{
    public class ServerJson
    {
        public string Country { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public IPEndPoint InternalEndpoint6 { get; set; }
        public string Version { get; set; }
        public bool Password { get; set; }
        public bool Cheats { get; set; }
        public int GameMode { get; set; }
        public int MaxPlayers { get; set; }
        public bool DedicatedServer { get; set; }
        public int PlayerCount { get; set; }
        public string ServerName { get; set; }
        public string Description { get; set; }
        public string Website { get; set; }
        public string WebsiteText { get; set; }

        public ServerJson(ServerInfo info)
        {
            Country = info.Country;
            ExternalEndpoint = info.ExternalEndpoint;
            InternalEndpoint6 = info.InternalEndpoint6;
            Version = info.ServerVersion;
            Password = info.Password;
            Cheats = info.Cheats;
            GameMode = info.GameMode;
            MaxPlayers = info.MaxPlayers;
            DedicatedServer = info.DedicatedServer;
            PlayerCount = info.PlayerCount;
            ServerName = info.ServerName;
            Description = info.Description;
            Website = info.Website;
            WebsiteText = info.WebsiteText;
        }
    }
}

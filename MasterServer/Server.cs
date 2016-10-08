using System;
using System.Net;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;

namespace MasterServer
{
    public class Server
    {
        public long LastRegisterTime { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public ServerInfo Info { get; set; }

        public Server(MsRegisterServerMsgData msg, IPEndPoint externalEndpoint)
        {
            var endpoint = msg.InternalEndpoint.Split(':');
            InternalEndpoint = new IPEndPoint(IPAddress.Parse(endpoint[0]), int.Parse(endpoint[1]));
            ExternalEndpoint = externalEndpoint;
            LastRegisterTime = DateTime.UtcNow.Ticks;
            Info = new ServerInfo
            {
                Id = msg.Id,
                Cheats = msg.Cheats,
                Description = msg.Description,
                DropControlOnExit = msg.DropControlOnExit,
                DropControlOnExitFlight = msg.DropControlOnExitFlight,
                DropControlOnVesselSwitching = msg.DropControlOnVesselSwitching,
                GameMode = msg.GameMode,
                MaxPlayers = msg.MaxPlayers,
                ModControl = msg.ModControl,
                PlayerCount = msg.PlayerCount,
                ServerName = msg.ServerName,
                Version = msg.Version,
                VesselUpdatesSendMsInterval = msg.VesselUpdatesSendMsInterval,
                WarpMode = msg.WarpMode
            };
        }
    }
}

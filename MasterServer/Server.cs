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
            InternalEndpoint = Common.CreateEndpointFromString(msg.InternalEndpoint);
            ExternalEndpoint = externalEndpoint;
            LastRegisterTime = DateTime.UtcNow.Ticks;
            Info = new ServerInfo
            {
                Id = msg.Id,
                Cheats = msg.Cheats,
                ShowVesselsInThePast = msg.ShowVesselsInThePast,
                Description = msg.Description,
                DropControlOnExit = msg.DropControlOnExit,
                DropControlOnExitFlight = msg.DropControlOnExitFlight,
                DropControlOnVesselSwitching = msg.DropControlOnVesselSwitching,
                GameMode = msg.GameMode,
                MaxPlayers = msg.MaxPlayers,
                ModControl = msg.ModControl,
                PlayerCount = msg.PlayerCount,
                ServerName = msg.ServerName,
                Version = msg.ServerVersion,
                WarpMode = msg.WarpMode
            };

            Info.ServerName = Info.ServerName.Length > 30 ? Info.ServerName.Substring(0, 30) : Info.ServerName;
            Info.Description = Info.Description.Length > 200 ? Info.Description.Substring(0, 200) : Info.Description;
        }
    }
}

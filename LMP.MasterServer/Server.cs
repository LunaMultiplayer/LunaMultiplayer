using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Time;
using System.Linq;
using System.Net;

namespace LMP.MasterServer
{
    public class Server
    {
        public long LastRegisterTime { get; set; }
        public IPEndPoint InternalEndpoint { get; set; }
        public IPEndPoint ExternalEndpoint { get; set; }
        public ServerInfo Info { get; set; }

        public Server(MsRegisterServerMsgData msg, IPEndPoint externalEndpoint)
        {
            if (IsLocalIpAddress(externalEndpoint.Address))
            {
                ExternalEndpoint = new IPEndPoint(IPAddress.Parse(Helper.GetOwnIpAddress()), externalEndpoint.Port);
            }
            else
            {
                ExternalEndpoint = externalEndpoint;
            }
            InternalEndpoint = Common.CreateEndpointFromString(msg.InternalEndpoint);
            LastRegisterTime = LunaTime.UtcNow.Ticks;
            Info = new ServerInfo
            {
                Id = msg.Id,
                ExternalEndpoint = $"{ExternalEndpoint.Address}:{ExternalEndpoint.Port}",
                InternalEndpoint = $"{InternalEndpoint.Address}:{InternalEndpoint.Port}",
                Cheats = msg.Cheats,
                ServerVersion = msg.ServerVersion,
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
                WarpMode = msg.WarpMode,
                TerrainQuality = msg.TerrainQuality
            };

            Info.ServerName = Info.ServerName.Length > 30 ? Info.ServerName.Substring(0, 30) : Info.ServerName;
            Info.Description = Info.Description.Length > 200 ? Info.Description.Substring(0, 200) : Info.Description;
        }

        public static bool IsLocalIpAddress(IPAddress host)
        {
            try
            { 
                var hostIPs = Dns.GetHostAddresses(host.ToString());
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

                // test if any host IP equals to any local IP or to localhost
                foreach (var hostIp in hostIPs)
                {
                    if (IPAddress.IsLoopback(hostIp)) return true;
                    if (localIPs.Any(l => l.Equals(hostIp))) return true;
                }
            }
            catch
            {
                // ignored
            }
            return false;
        }
    }
}

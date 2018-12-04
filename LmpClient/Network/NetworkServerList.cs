using Lidgren.Network;
using LmpClient.Systems.Ping;
using LmpCommon;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Message.MasterServer;
using LmpCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Net;
using UniLinq;

namespace LmpClient.Network
{
    public class NetworkServerList
    {
        public static string Password { get; set; } = string.Empty;

        private static string LastIntroductionToken { get; set; }

        public static ConcurrentDictionary<long, ServerInfo> Servers { get; } = new ConcurrentDictionary<long, ServerInfo>();
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// Sends a request servers to the master servers
        /// </summary>
        public static void RequestServers()
        {
            Servers.Clear();
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsRequestServersMsgData>();
            var requestMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);
            NetworkSender.QueueOutgoingMessage(requestMsg);
        }

        /// <summary>
        /// Handles a server list response from the master servers
        /// </summary>
        public static void HandleServersList(NetIncomingMessage msg)
        {
            try
            {
                var msgDeserialized = NetworkMain.MstSrvMsgFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks);
                
                //Sometimes we receive other type of unconnected messages. 
                //Therefore we assert that the received message data is of MsReplyServersMsgData
                if (msgDeserialized.Data is MsReplyServersMsgData data)
                {
                    //Filter servers with different version
                    if (!LmpVersioning.IsCompatible(data.ServerVersion))
                        return;

                    if (!Servers.ContainsKey(data.Id))
                    {
                        var server = new ServerInfo
                        {
                            Id = data.Id,
                            InternalEndpoint = data.InternalEndpoint,
                            ExternalEndpoint = data.ExternalEndpoint,
                            Description = data.Description,
                            Country = data.Country,
                            Website = data.Website,
                            WebsiteText = data.WebsiteText,
                            Password = data.Password,
                            Cheats = data.Cheats,
                            ServerName = data.ServerName,
                            DropControlOnExit = data.DropControlOnExit,
                            MaxPlayers = data.MaxPlayers,
                            WarpMode = data.WarpMode,
                            TerrainQuality = data.TerrainQuality,
                            PlayerCount = data.PlayerCount,
                            GameMode = data.GameMode,
                            ModControl = data.ModControl,
                            DedicatedServer = data.DedicatedServer,
                            DropControlOnExitFlight = data.DropControlOnExitFlight,
                            VesselUpdatesSendMsInterval = data.VesselUpdatesSendMsInterval,
                            DropControlOnVesselSwitching = data.DropControlOnVesselSwitching,
                            ServerVersion = data.ServerVersion
                        };

                        if (Servers.TryAdd(data.Id, server))
                            PingSystem.QueuePing(data.Id);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Invalid server list reply msg: {e}");
            }
        }

        /// <summary>
        /// Send a request to the master server to introduce us and do the nat punchtrough to the selected server
        /// </summary>
        public static void IntroduceToServer(long serverId)
        {
            if (Servers.TryGetValue(serverId, out var serverInfo))
            {
                if (ServerIsInLocalLan(serverInfo.ExternalEndpoint))
                {
                    LunaLog.Log("Server is in LAN. Skipping NAT punch");
                    NetworkConnection.ConnectToServer(serverInfo.InternalEndpoint, Password);
                }
                else
                {
                    try
                    {
                        var token = RandomString(10);

                        var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsIntroductionMsgData>();
                        msgData.Id = serverId;
                        msgData.Token = token;
                        msgData.InternalEndpoint = new IPEndPoint(LunaNetUtils.GetMyAddress(), NetworkMain.Config.Port);

                        var introduceMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);

                        LunaLog.Log($"[LMP]: Sending NAT introduction to server. Token: {token}");
                        NetworkSender.QueueOutgoingMessage(introduceMsg);
                    }
                    catch (Exception e)
                    {
                        LunaLog.LogError($"[LMP]: Error connecting to server: {e}");
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the server is running in a local LAN
        /// </summary>
        private static bool ServerIsInLocalLan(IPEndPoint serverEndPoint)
        {
            return Equals(LunaNetUtils.GetOwnExternalIpAddress(), serverEndPoint.Address);
        }

        /// <summary>
        /// We received a nat punchtrough response so connect to the server
        /// </summary>
        public static void HandleNatIntroduction(NetIncomingMessage msg)
        {
            try
            {
                var token = msg.ReadString();
                if (LastIntroductionToken != token)
                {
                    LastIntroductionToken = token;
                    LunaLog.Log($"[LMP]: Nat introduction success to {msg.SenderEndPoint} token is: {token}");
                    NetworkConnection.ConnectToServer(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port, Password);
                }
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling NAT introduction: {e}");
            }
        }

        /// <summary>
        /// Generates a random string, useful for token
        /// </summary>
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}

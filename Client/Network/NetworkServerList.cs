using Lidgren.Network;
using LunaClient.Systems.Ping;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using UniLinq;
using Random = System.Random;

namespace LunaClient.Network
{
    public class NetworkServerList
    {
        private static readonly List<IPEndPoint> PrivMasterServers = new List<IPEndPoint>();
        public static List<IPEndPoint> MasterServers
        {
            get
            {
                lock (PrivMasterServers)
                {
                    if (!PrivMasterServers.Any())
                    {
                        var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
                        PrivMasterServers.AddRange(servers.Select(Common.CreateEndpointFromString));
                    }
                    return PrivMasterServers;
                }
            }
        }

        public static ConcurrentDictionary<string, ServerInfo> Servers { get; } = new ConcurrentDictionary<string, ServerInfo>();
        private static readonly Random Random = new Random();
        
        /// <summary>
        /// Sends a request servers to the master servers
        /// </summary>
        public static void RequestServers()
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsRequestServersMsgData>();
            msgData.CurrentVersion = LmpVersioning.CurrentVersion;

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
                var msgDeserialized = NetworkMain.MstSrvMsgFactory.Deserialize(msg, LunaTime.UtcNow.Ticks);
                
                //Sometimes we receive other type of unconnected messages. 
                //Therefore we assert that the received message data is of MsReplyServersMsgData
                if (msgDeserialized.Data is MsReplyServersMsgData data)
                {
                    Servers.Clear();
                    for (var i = 0; i < data.ServersCount; i++)
                    {
                        //Filter servers with diferent version
                        if (data.ServerVersion[i] != LmpVersioning.CurrentVersion)
                            continue;

                        PingSystem.QueuePing(data.ExternalEndpoint[i]);
                        Servers.TryAdd(data.ExternalEndpoint[i], new ServerInfo
                        {
                            Id = data.Id[i],
                            InternalEndpoint = data.InternalEndpoint[i],
                            ExternalEndpoint = data.ExternalEndpoint[i],
                            Description = data.Description[i],
                            Cheats = data.Cheats[i],
                            ServerName = data.ServerName[i],
                            DropControlOnExit = data.DropControlOnExit[i],
                            MaxPlayers = data.MaxPlayers[i],
                            WarpMode = data.WarpMode[i],
                            TerrainQuality = data.TerrainQuality[i],
                            PlayerCount = data.PlayerCount[i],
                            GameMode = data.GameMode[i],
                            ModControl = data.ModControl[i],
                            DropControlOnExitFlight = data.DropControlOnExitFlight[i],
                            VesselUpdatesSendMsInterval = data.VesselUpdatesSendMsInterval[i],
                            DropControlOnVesselSwitching = data.DropControlOnVesselSwitching[i],
                            ServerVersion = data.ServerVersion[i]
                        });
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
        public static void IntroduceToServer(long currentEntryId)
        {
            try
            {
                var token = RandomString(10);
                var ownEndpoint = new IPEndPoint(LunaNetUtils.GetMyAddress(), NetworkMain.Config.Port);

                var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsIntroductionMsgData>();
                msgData.Id = currentEntryId;
                msgData.Token = token;
                msgData.InternalEndpoint = Common.StringFromEndpoint(ownEndpoint);

                var introduceMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);

                LunaLog.Log($"[LMP]: Sending NAT introduction to server. Token: {token}");
                NetworkSender.QueueOutgoingMessage(introduceMsg);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error connecting to server: {e}");
            }
        }

        /// <summary>
        /// We received a nat punchtrough response so connect to the server
        /// </summary>
        public static void HandleNatIntroduction(NetIncomingMessage msg)
        {
            try
            {
                var token = msg.ReadString();
                LunaLog.Log($"[LMP]: Nat introduction success to {msg.SenderEndPoint} token is: {token}");
                NetworkConnection.ConnectToServer(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
            }
            catch (Exception e)
            {
                LunaLog.LogError($"[LMP]: Error handling NAT introduction: {e}");
            }
        }

        /// <summary>
        /// Generates a random string, usefull for token
        /// </summary>
        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}

using Lidgren.Network;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using System;
using System.Collections.Generic;
using System.Net;
using UniLinq;
using Random = System.Random;

namespace LunaClient.Network
{
    public class NetworkServerList
    {
        public static List<IPEndPoint> MasterServers { get; } = new List<IPEndPoint>();
        public static List<ServerInfo> Servers { get; private set; } = new List<ServerInfo>();
        private static readonly Random Random = new Random();

        /// <summary>
        /// Refreshes the list of master servers
        /// </summary>
        public static void RefreshMasterServers()
        {
            if (!MasterServers.Any())
            {
                var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
                foreach (var server in servers)
                {
                    MasterServers.Add(Common.CreateEndpointFromString(server));
                }
            }
        }

        /// <summary>
        /// Sends a request servers to the master servers
        /// </summary>
        public static void RequestServers()
        {
            var requestMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(new MsRequestServersMsgData { CurrentVersion = VersionInfo.VersionNumber });
            NetworkSender.QueueOutgoingMessage(requestMsg);
        }

        /// <summary>
        /// Handles a server list response from the master servers
        /// </summary>
        public static void HandleServersList(NetIncomingMessage msg)
        {
            try
            {
                var msgDeserialized = NetworkMain.MstSrvMsgFactory.Deserialize(msg.ReadBytes(msg.LengthBytes), DateTime.UtcNow.Ticks);
                if (msgDeserialized != null)
                {

                    //Sometimes we receive other type of unconnected messages. 
                    //Therefore we assert that the received message data is of MsReplyServersMsgData
                    if (msgDeserialized.Data is MsReplyServersMsgData data)
                    {
                        Servers.Clear();

                        for (var i = 0; i < data.Id.Length; i++)
                        {
                            Servers.Add(new ServerInfo
                            {
                                Id = data.Id[i],
                                Description = data.Description[i],
                                Cheats = data.Cheats[i],
                                ServerName = data.ServerName[i],
                                DropControlOnExit = data.DropControlOnExit[i],
                                MaxPlayers = data.MaxPlayers[i],
                                WarpMode = data.WarpMode[i],
                                PlayerCount = data.PlayerCount[i],
                                GameMode = data.GameMode[i],
                                ModControl = data.ModControl[i],
                                DropControlOnExitFlight = data.DropControlOnExitFlight[i],
                                VesselUpdatesSendMsInterval = data.VesselUpdatesSendMsInterval[i],
                                DropControlOnVesselSwitching = data.DropControlOnVesselSwitching[i],
                                Version = data.Version
                            });
                        }

                        Servers = Servers.OrderBy(s => s.ServerName).ToList();
                    }
                }
                else
                {
                    LunaLog.LogError($"[LMP]: Unable to deserialize message: {msg}");
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
            var token = RandomString(10);
            var ownEndpoint = new IPEndPoint(NetUtility.GetMyAddress(out var _), NetworkMain.Config.Port);

            var introduceMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(new MsIntroductionMsgData
            {
                Id = currentEntryId,
                Token = token,
                InternalEndpoint = Common.StringFromEndpoint(ownEndpoint)
            });

            LunaLog.Log($"[LMP]: Sending NAT introduction to server. Token: {token}");
            NetworkSender.QueueOutgoingMessage(introduceMsg);
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

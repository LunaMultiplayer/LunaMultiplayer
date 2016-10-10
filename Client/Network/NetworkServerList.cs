using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using UniLinq;
using UnityEngine;
using Random = System.Random;

namespace LunaClient.Network
{
    public class NetworkServerList
    {
        public static List<IPEndPoint> MasterServers { get; } = new List<IPEndPoint>();
        public static List<ServerInfo> Servers { get; private set; } = new List<ServerInfo>();
        private static readonly Random Random = new Random();

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
            var requestMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(new MsRequestServersMsgData());
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
                var data = msgDeserialized.Data as MsReplyServersMsgData;

                if (data != null)
                {
                    for (var i = 0; i < data.Id.Length; i++)
                    {
                        var id = data.Id[i];
                        if (!Servers.Any(s => s.Id == id))
                        {
                            Servers.Add(new ServerInfo
                            {
                                Id = id,
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
                    }

                    Servers = Servers.OrderBy(s => s.ServerName).ToList();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Invalid server list reply msg: {e}");
            }
        }

        public static void IntroduceToServer(long currentEntryId)
        {
            var token = RandomString(10);
            IPAddress mask;
            var ownEndpoint = new IPEndPoint(NetUtility.GetMyAddress(out mask), NetworkMain.Config.Port);

            var introduceMsg = NetworkMain.MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(new MsIntroductionMsgData
            {
                Id = currentEntryId,
                Token = token,
                InternalEndpoint = Common.StringFromEndpoint(ownEndpoint)
            });

            Debug.Log($"Sending NAT introduction to server. Token: {token}");
            NetworkSender.QueueOutgoingMessage(introduceMsg);
        }

        public static void HandleNatIntroduction(NetIncomingMessage msg)
        {
            try
            {
                var token = msg.ReadString();
                Debug.Log($"Nat introduction success to {msg.SenderEndPoint} token is: {token}");
                NetworkConnection.ConnectToServer(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
            }
            catch (Exception e)
            {
                Debug.Log($"Error handling NAT introduction: {e}");
            }
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }
    }
}

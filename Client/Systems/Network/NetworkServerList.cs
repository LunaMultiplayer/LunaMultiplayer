using System;
using System.Collections.Generic;
using System.Net;
using Lidgren.Network;
using LunaClient.Utilities;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using UniLinq;

namespace LunaClient.Systems.Network
{
    public partial class NetworkSystem
    {
        public List<IPEndPoint> MasterServers { get; } = new List<IPEndPoint>();
        public List<ServerInfo> Servers { get; private set; } = new List<ServerInfo>();

        private void RefreshMasterServers()
        {
            var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
            foreach (var server in servers)
            {
                MasterServers.Add(Common.CreateEndpointFromString(server));
            }
        }

        /// <summary>
        /// Sends a request servers to the master servers
        /// </summary>
        public void RequestServers()
        {
            if (!MasterServers.Any())
                RefreshMasterServers();

            var requestMsg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(new MsRequestServersMsgData());
            var requestMsgBytes = MasterServerMessageFactory.Serialize(requestMsg);

            var outMsg = ClientConnection.CreateMessage(requestMsgBytes.Length);
            outMsg.Write(requestMsgBytes);
            
            foreach (var masterServer in MasterServers)
            {
                ClientConnection.SendUnconnectedMessage(outMsg, masterServer);
            }
        }

        /// <summary>
        /// Handles a server list response from the master servers
        /// </summary>
        private void HandleServersList(NetIncomingMessage msg)
        {
            var servers = MasterServerMessageFactory
                .Deserialize(msg.ReadBytes(msg.LengthBytes), DateTime.UtcNow.Ticks) as MainMstSrvMsg;
            var data = servers?.Data as MsReplyServersMsgData;

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
            }

            Servers = Servers.OrderBy(s => s.ServerName).ToList();
        }

        public void IntroduceToServer(long currentEntryId)
        {
            var token = "EXAMPLETOKEN";

            IPAddress mask;
            var ownEndpoint = new IPEndPoint(NetUtility.GetMyAddress(out mask), Config.Port);

            var introduceMsg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(new MsIntroductionMsgData
            {
                Id = currentEntryId, Token = token, InternalEndpoint = Common.StringFromEndpoint(ownEndpoint)
            });

            var introduceMsgBytes = MasterServerMessageFactory.Serialize(introduceMsg);

            var outMsg = ClientConnection.CreateMessage(introduceMsgBytes.Length);
            outMsg.Write(introduceMsgBytes);

            foreach (var masterServer in MasterServers)
            {
                ClientConnection.SendUnconnectedMessage(outMsg, masterServer);
            }
        }

        private void HandleNatIntroduction(NetIncomingMessage msg)
        {
            var token = msg.ReadString();
            LunaLog.Debug("Nat introduction success to " + msg.SenderEndPoint + " token is: " + token);
            ConnectToServer(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
        }
    }
}

using Lidgren.Network;
using LunaClient.Base;
using LunaClient.Systems.Ping;
using LunaCommon;
using LunaCommon.Message;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.MasterServer;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using UniLinq;

namespace LunaClient.Network
{
    public class NetworkMasterServer
    {
        private static readonly Random Random = new Random();
        public static MasterServerMessageFactory MstSrvMsgFactory { get; } = new MasterServerMessageFactory();

        public static NetPeerConfiguration Config { get; } = new NetPeerConfiguration("LMP")
        {
            UseMessageRecycling = true,
            ReceiveBufferSize = 500000, //500Kb
            SendBufferSize = 500000, //500Kb
            AutoFlushSendQueue = false,
            PingInterval = 2.5f,
            ConnectionTimeout = 15,
        };
        
        private static ConcurrentDictionary<IPEndPoint, NetClient> MasterServerEndpoints { get; } = new ConcurrentDictionary<IPEndPoint, NetClient>();

        public static void AwakeMasterServerSystem()
        {
            Config.EnableMessageType(NetIncomingMessageType.Data);
        }
        
        public static void RefreshMasterServersList()
        {
            SystemBase.TaskFactory.StartNew(() =>
            {
                foreach (var masterServer in MasterServerRetriever.RetrieveWorkingMasterServersEndpoints())
                {
                    var endpoint = Common.CreateEndpointFromString(masterServer);

                    var netClient = new NetClient(Config);
                    netClient.Start();

                    netClient.Connect(endpoint);
                    netClient.FlushSendQueue();

                    var connectionTrials = 0;
                    while (netClient.ConnectionStatus != NetConnectionStatus.Connected && connectionTrials < 3)
                    {
                        connectionTrials++;
                        Thread.Sleep(1000);
                    }

                    if (netClient.ConnectionStatus == NetConnectionStatus.Connected)
                    {
                        MasterServerEndpoints.TryAdd(endpoint, netClient);
                        RequestServerList(netClient);
                        netClient.RegisterReceivedCallback(GotMessage);
                    }
                }
            });
        }

        public static void RequestServerList()
        {
            foreach (var masterServer in MasterServerEndpoints.Values.ToArray())
            {
                RequestServerList(masterServer);
            }
        }

        public static void RequestServerList(NetClient masterServer)
        {
            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsRequestServersMsgData>();
            var requestMsg = MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);

            var lidgrenMsg = masterServer.CreateMessage(requestMsg.GetMessageSize());
            requestMsg.Serialize(lidgrenMsg);

            masterServer.SendMessage(lidgrenMsg, requestMsg.NetDeliveryMethod);
            masterServer.FlushSendQueue();

            requestMsg.Recycle();
        }

        public static void IntroduceToServer(long currentEntryId)
        {
            var token = RandomString(10);
            var ownEndpoint = new IPEndPoint(LunaNetUtils.GetMyAddress(), NetworkMain.Config.Port);
            LunaLog.Log($"[LMP]: Sending NAT introduction to server. Token: {token}");

            var msgData = NetworkMain.CliMsgFactory.CreateNewMessageData<MsIntroductionMsgData>();
            msgData.Id = currentEntryId;
            msgData.Token = token;
            msgData.InternalEndpoint = Common.StringFromEndpoint(ownEndpoint);
            var introduceMsg = MstSrvMsgFactory.CreateNew<MainMstSrvMsg>(msgData);

            foreach (var masterServer in MasterServerEndpoints.Values.ToArray())
            {
                try
                {
                    var lidgrenMsg = masterServer.CreateMessage((int) introduceMsg.GetMessageSize());
                    introduceMsg.Serialize(lidgrenMsg);

                    masterServer.SendMessage(lidgrenMsg, introduceMsg.NetDeliveryMethod);
                    masterServer.FlushSendQueue();
                }
                catch (Exception e)
                {
                    LunaLog.LogError($"[LMP]: Error connecting to server: {e}");
                }
            }

            introduceMsg.Recycle();
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        private static void GotMessage(object peer)
        {
            NetIncomingMessage im;
            while ((im = (peer as NetPeer)?.ReadMessage()) != null)
            {
                // handle incoming message
                switch (im.MessageType)
                {
                    case NetIncomingMessageType.Data:
                        HandleServersList(im);
                        break;
                    case NetIncomingMessageType.NatIntroductionSuccess:
                        HandleNatIntroduction(im);
                        break;
                }
            }
        }

        /// <summary>
        /// We received a nat punchtrough response so connect to the server
        /// </summary>
        private static void HandleNatIntroduction(NetIncomingMessage msg)
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

        private static void HandleServersList(NetIncomingMessage msg)
        {
            try
            {
                var msgDeserialized = MstSrvMsgFactory.Deserialize(msg, LunaTime.UtcNow.Ticks);

                //Sometimes we receive other type of unconnected messages. 
                //Therefore we assert that the received message data is of MsReplyServersMsgData
                if (msgDeserialized.Data is MsReplyServersMsgData data)
                {
                    NetworkServerList.Servers.Clear();
                    for (var i = 0; i < data.ServersCount; i++)
                    {
                        //Filter servers with diferent version
                        if (data.ServerVersion[i] != LmpVersioning.CurrentVersion)
                            continue;
                        
                        PingSystem.QueuePing(data.ExternalEndpoint[i]);
                        NetworkServerList.Servers.TryAdd(data.ExternalEndpoint[i], new ServerInfo
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
    }
}

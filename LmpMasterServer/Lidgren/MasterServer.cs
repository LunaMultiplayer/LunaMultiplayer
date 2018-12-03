using Lidgren.Network;
using LmpCommon;
using LmpCommon.Message;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Message.Interface;
using LmpCommon.Message.MasterServer;
using LmpCommon.Message.Types;
using LmpCommon.Time;
using LmpGlobal;
using LmpMasterServer.Structure;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLogger = LmpCommon.ConsoleLogger;
using LogLevels = LmpCommon.LogLevels;

namespace LmpMasterServer.Lidgren
{
    public class MasterServer
    {
        public static int ServerMsTick { get; set; } = 100;
        public static int ServerMsTimeout { get; set; } = 15000;
        public static int ServerRemoveMsCheckInterval { get; set; } = 5000;
        public static ushort Port { get; set; } = 8700;
        public static bool RunServer { get; set; }
        public static ConcurrentDictionary<long, Server> ServerDictionary { get; } = new ConcurrentDictionary<long, Server>();
        private static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();

        public static async void Start()
        {
            var config = new NetPeerConfiguration("masterserver")
            {
                Port = Port,
                SuppressUnreliableUnorderedAcks = true,
                PingInterval = 500,
                ConnectionTimeout = ServerMsTimeout
            };

            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            var peer = new NetServer(config);
            peer.Start();

            CheckMasterServerListed();

            ConsoleLogger.Log(LogLevels.Normal, $"Master server {LmpVersioning.CurrentVersion} started! Поехали!");
            RemoveExpiredServers();

            while (RunServer)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            ConsoleLogger.Log(LogLevels.Debug, msg.ReadString());
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            ConsoleLogger.Log(LogLevels.Warning, msg.ReadString());
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            ConsoleLogger.Log(LogLevels.Error, msg.ReadString());
                            break;
                        case NetIncomingMessageType.UnconnectedData:
                            var message = GetMessage(msg);
                            if (message != null && !message.VersionMismatch)
                            {
                                HandleMessage(message, msg, peer);
                            }
                            break;
                    }
                }
                await Task.Delay(ServerMsTick);
            }
            peer.Shutdown("So long and thanks for all the fish!");
        }

        private static IMasterServerMessageBase GetMessage(NetIncomingMessage msg)
        {
            try
            {
                var message = MasterServerMessageFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks) as IMasterServerMessageBase;
                return message;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void CheckMasterServerListed()
        {
            var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
            var ownEndpoint = $"{LunaNetUtils.GetOwnExternalIpAddress()}:{Port}";

            if(!servers.Contains(ownEndpoint))
            {
                ConsoleLogger.Log(LogLevels.Error, $"You're not in the master-servers URL ({RepoConstants.MasterServersListShortUrl}) " +
                    "Clients/Servers won't see you");
            }
            else
            {
                ConsoleLogger.Log(LogLevels.Normal, "Own ip correctly listed in master - servers URL");
            }
        }


        private static void HandleMessage(IMasterServerMessageBase message, NetIncomingMessage netMsg, NetPeer peer)
        {
            if (BannedIpsRetriever.IsBanned(netMsg.SenderEndPoint))
                return;

            try
            {
                switch ((message?.Data as MsBaseMsgData)?.MasterServerMessageSubType)
                {
                    case MasterServerMessageSubType.RegisterServer:
                        RegisterServer(message, netMsg);
                        break;
                    case MasterServerMessageSubType.RequestServers:
                        ConsoleLogger.Log(LogLevels.Normal, $"LIST REQUEST from: {netMsg.SenderEndPoint}");
                        SendServerLists(netMsg, peer);
                        break;
                    case MasterServerMessageSubType.Introduction:
                        var msgData = (MsIntroductionMsgData)message.Data;
                        if (ServerDictionary.TryGetValue(msgData.Id, out var server))
                        {
                            ConsoleLogger.Log(LogLevels.Normal, $"INTRODUCTION request from: {netMsg.SenderEndPoint} to server: {server.ExternalEndpoint}");
                            peer.Introduce(server.InternalEndpoint, server.ExternalEndpoint,
                                Common.CreateEndpointFromString(msgData.InternalEndpoint),// client internal
                                netMsg.SenderEndPoint,// client external
                                msgData.Token); // request token
                        }
                        else
                        {
                            ConsoleLogger.Log(LogLevels.Warning, $"Client {netMsg.SenderEndPoint} requested introduction to nonlisted host!");
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ConsoleLogger.Log(LogLevels.Error, $"Error handling message. Details: {e}");
            }
        }

        /// <summary>
        /// Return the list of servers that match the version specified
        /// </summary>
        private static void SendServerLists(NetIncomingMessage netMsg, NetPeer peer)
        {
            foreach (var server in ServerDictionary.Values.ToArray())
            {
                var msgData = MasterServerMessageFactory.CreateNewMessageData<MsReplyServersMsgData>();

                msgData.Id = server.Info.Id;
                msgData.ServerVersion = server.Info.ServerVersion;
                msgData.Password = server.Info.Password;
                msgData.Cheats = server.Info.Cheats;
                msgData.Description = server.Info.Description;
                msgData.Country = server.Info.Country;
                msgData.Website = server.Info.Website;
                msgData.WebsiteText = server.Info.WebsiteText;
                msgData.DropControlOnExit = server.Info.DropControlOnExit;
                msgData.DropControlOnExitFlight = server.Info.DropControlOnExitFlight;
                msgData.DropControlOnVesselSwitching = server.Info.DropControlOnVesselSwitching;
                msgData.ExternalEndpoint = $"{server.ExternalEndpoint.Address}:{server.ExternalEndpoint.Port}";
                msgData.GameMode = server.Info.GameMode;
                msgData.InternalEndpoint = $"{server.InternalEndpoint.Address}:{server.InternalEndpoint.Port}";
                msgData.MaxPlayers = server.Info.MaxPlayers;
                msgData.ModControl = server.Info.ModControl;
                msgData.DedicatedServer = server.Info.DedicatedServer;
                msgData.PlayerCount = server.Info.PlayerCount;
                msgData.ServerName = server.Info.ServerName;
                msgData.VesselUpdatesSendMsInterval = server.Info.VesselUpdatesSendMsInterval;
                msgData.WarpMode = server.Info.WarpMode;
                msgData.TerrainQuality = server.Info.TerrainQuality;

                var msg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                var outMsg = peer.CreateMessage(msg.GetMessageSize());

                msg.Serialize(outMsg);
                peer.SendUnconnectedMessage(outMsg, netMsg.SenderEndPoint);

                //Force send of packets
                peer.FlushSendQueue();
            }
        }

        private static void RegisterServer(IMessageBase message, NetIncomingMessage netMsg)
        {
            var msgData = (MsRegisterServerMsgData)message.Data;

            if (!ServerDictionary.ContainsKey(msgData.Id))
            {
                ServerDictionary.TryAdd(msgData.Id, new Server(msgData, netMsg.SenderEndPoint));
                ConsoleLogger.Log(LogLevels.Normal, $"NEW SERVER: {netMsg.SenderEndPoint}");
            }
            else
            {
                //Just update
                ServerDictionary[msgData.Id].Update(msgData);
            }
        }

        private static void RemoveExpiredServers()
        {
            Task.Run(async () =>
            {
                while (RunServer)
                {
                    var serversIdsToRemove = ServerDictionary
                        .Where(s => LunaNetworkTime.UtcNow.Ticks - s.Value.LastRegisterTime >
                                    TimeSpan.FromMilliseconds(ServerMsTimeout).Ticks || 
                                    BannedIpsRetriever.IsBanned(s.Value.ExternalEndpoint))
                        .ToArray();

                    foreach (var serverId in serversIdsToRemove)
                    {
                        ConsoleLogger.Log(LogLevels.Normal, $"REMOVING SERVER: {serverId.Value.ExternalEndpoint}");
                        ServerDictionary.TryRemove(serverId.Key, out var _);
                    }

                    await Task.Delay(ServerRemoveMsCheckInterval);
                }
            });
        }
    }
}

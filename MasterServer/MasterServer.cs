using Lidgren.Network;
using LunaCommon;
using LunaCommon.Message;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaCommon.Message.Types;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MasterServer
{
    public class MasterServer
    {
        public static int ServerMsTimeout { get; set; } = 15000;
        public static int ServerRemoveMsCheckInterval { get; set; } = 5000;
        private static long LastServerExpireCheck { get; set; }
        public static ushort Port { get; set; }
        public static bool RunServer { get; set; }
        public static ConcurrentDictionary<long, Server> ServerDictionary { get; } = new ConcurrentDictionary<long, Server>();
        private static MasterServerForm Form { get; set; }
        private static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();

        public static void Start(MasterServerForm form)
        {
            RunServer = true;
            Form = form;

            var config = new NetPeerConfiguration("masterserver");
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.Port = Port;

            var peer = new NetPeer(config);
            peer.Start();

            Form.WriteLine("Master server started!");
            Task.Run(() => RemoveExpiredServers());

            CheckMasterServerListed();

            while (RunServer)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Form.WriteLine($"ERROR! :{msg.ReadString()}");
                            break;
                        case NetIncomingMessageType.UnconnectedData:
                            var messageBytes = msg.ReadBytes(msg.LengthBytes);
                            var message = GetMessage(messageBytes);
                            if (message!= null)
                                HandleMessage(message, msg, peer);
                            break;
                    }
                }
            }
            peer.Shutdown("shutting down");
        }

        private static IMasterServerMessageBase GetMessage(byte[] messageBytes)
        {
            try
            {
                var message = MasterServerMessageFactory.Deserialize(messageBytes, DateTime.UtcNow.Ticks) as IMasterServerMessageBase;
                return message;
            }
            catch (Exception e)
            {
                Form.WriteLine($"ERROR deserializing message! :{e}");
                return null;
            }
        }

        private static void CheckMasterServerListed()
        {
            var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
            var ownEndpoint = $"{GetOwnIpAddress()}:{Port}";

            Form.WriteLine(!servers.Contains(ownEndpoint)
                ? "CAUTION! This server is not listed in the master-servers URL " +
                  $"({MasterServerRetriever.MasterServersListShortUrl}) Clients/Servers will not see you"
                : $"Own ip correctly listed in master-servers URL ({MasterServerRetriever.MasterServersListShortUrl})");
        }

        private static string GetOwnIpAddress()
        {
            var currentIpAddress = "";

            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://ip.42.pl/raw");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("https://api.ipify.org/");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://httpbin.org/ip");
            if (string.IsNullOrEmpty(currentIpAddress))
                currentIpAddress = TryGetIpAddress("http://checkip.dyndns.org");

            return currentIpAddress;
        }

        private static string TryGetIpAddress(string url)
        {
            using (var client = new WebClient())
            using (var stream = client.OpenRead(url))
            {
                if (stream == null) return null;
                using (var reader = new StreamReader(stream))
                {
                    var ipRegEx = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                    var result = ipRegEx.Matches(reader.ReadToEnd());

                    if (IPAddress.TryParse(result[0].Value, out var ip))
                        return ip.ToString();
                }
            }
            return null;
        }

        private static void HandleMessage(IMasterServerMessageBase message, NetIncomingMessage netMsg, NetPeer peer)
        {
            switch ((message?.Data as MsBaseMsgData)?.MasterServerMessageSubType)
            {
                case MasterServerMessageSubType.RegisterServer:
                    RegisterServer(message, netMsg);
                    break;
                case MasterServerMessageSubType.RequestServers:
                    var version = ((MsRequestServersMsgData)message.Data).CurrentVersion;
#if DEBUG
                    Form.WriteLine($"Received LIST REQUEST from: {netMsg.SenderEndPoint} version: {version}");
#endif
                    SendServerLists(netMsg, peer, version);
                    break;
                case MasterServerMessageSubType.Introduction:
#if DEBUG
                    Form.WriteLine($"Received INTRODUCTION request from: {netMsg.SenderEndPoint}");
#endif
                    var msgData = (MsIntroductionMsgData)message.Data;
                    Server server;
                    if (ServerDictionary.TryGetValue(msgData.Id, out server))
                    {
                        peer.Introduce(
                            server.InternalEndpoint,
                            server.ExternalEndpoint,
                            Common.CreateEndpointFromString(msgData.InternalEndpoint),// client internal
                            netMsg.SenderEndPoint,// client external
                            msgData.Token); // request token
                    }
                    else
                    {
                        Form.WriteLine("Client requested introduction to nonlisted host!");
                    }
                    break;
            }
        }

        /// <summary>
        /// Return the list of servers that match the version specified
        /// </summary>
        private static void SendServerLists(NetIncomingMessage netMsg, NetPeer peer, string version)
        {
            var values = ServerDictionary.Values.Where(s => s.Info.Version == version).OrderBy(v => v.Info.Id).ToArray();

            var msgData = MasterServerMessageFactory.CreateNewMessageData<MsReplyServersMsgData>();

            msgData.Id = values.Select(s => s.Info.Id).ToArray();
            msgData.Ip = values.Select(s => s.Info.Ip).ToArray();
            msgData.Cheats = values.Select(s => s.Info.Cheats).ToArray();
            msgData.Description = values.Select(s => s.Info.Description).ToArray();
            msgData.DropControlOnExit = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnExitFlight = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnVesselSwitching = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.ExternalEndpoint = values.Select(s => $"{s.ExternalEndpoint.Address}:{s.ExternalEndpoint.Port}").ToArray();
            msgData.GameMode = values.Select(s => s.Info.GameMode).ToArray();
            msgData.InternalEndpoint = values.Select(s => $"{s.InternalEndpoint.Address}:{s.InternalEndpoint.Port}").ToArray();
            msgData.MaxPlayers = values.Select(s => s.Info.MaxPlayers).ToArray();
            msgData.ModControl = values.Select(s => s.Info.ModControl).ToArray();
            msgData.PlayerCount = values.Select(s => s.Info.PlayerCount).ToArray();
            msgData.ServerName = values.Select(s => s.Info.ServerName).ToArray();
            msgData.VesselUpdatesSendMsInterval = values.Select(s => s.Info.VesselUpdatesSendMsInterval).ToArray();
            msgData.WarpMode = values.Select(s => s.Info.WarpMode).ToArray();

            var msg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
            var data = msg.Serialize(true);

            var outMsg = peer.CreateMessage(data.Length);
            outMsg.Write(data);
            peer.SendUnconnectedMessage(outMsg, netMsg.SenderEndPoint);
            peer.FlushSendQueue();
        }

        private static void RegisterServer(IMessageBase message, NetIncomingMessage netMsg)
        {
            var msgData = (MsRegisterServerMsgData)message.Data;

            if (!ServerDictionary.ContainsKey(msgData.Id))
            {
                ServerDictionary.TryAdd(msgData.Id, new Server(msgData, netMsg.SenderEndPoint));
                Form.UpdateServerList(ServerDictionary.Values);
            }
            else
            {
                //Just update
                ServerDictionary[msgData.Id] = new Server(msgData, netMsg.SenderEndPoint);
            }
        }

        private static void RemoveExpiredServers()
        {
            while (RunServer)
            {
                if (DateTime.UtcNow.Ticks - LastServerExpireCheck > TimeSpan.FromMilliseconds(ServerRemoveMsCheckInterval).Ticks)
                {
                    LastServerExpireCheck = DateTime.UtcNow.Ticks;

                    var serversIdsToRemove = ServerDictionary
                        .Where(s => DateTime.UtcNow.Ticks - s.Value.LastRegisterTime >
                                TimeSpan.FromMilliseconds(ServerMsTimeout).Ticks)
                        .Select(s => s.Key)
                        .ToArray();

                    foreach (var serverId in serversIdsToRemove)
                    {
                        ServerDictionary.TryRemove(serverId, out var _);
                    }

                    if (serversIdsToRemove.Any())
                        Form.UpdateServerList(ServerDictionary.Values);
                }
            }
        }
    }
}

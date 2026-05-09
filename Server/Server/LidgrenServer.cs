using Lidgren.Network;
using LmpCommon;
using LmpCommon.Enums;
using LmpCommon.Message.Data.MasterServer;
using LmpCommon.Message.Interface;
using LmpCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Settings.Structures;
using Server.Utilities;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server.Server
{
    public class LidgrenServer
    {
        public static NetServer Server { get; private set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        public static void SetupLidgrenServer()
        {
            // ListenAddress and socket dual-stacking logic
            // Try to parse the set address, error if it fails
            if (!IPAddress.TryParse(ConnectionSettings.SettingsStore.ListenAddress, out var listenAddress))
            {
                LunaLog.Error($"Could not parse ListenAddress, falling back on {(Socket.OSSupportsIPv6 ? "[::]" : "0.0.0.0")}.");
                // Fall back on whatever unspecified address we can
                listenAddress = Socket.OSSupportsIPv6 ? IPAddress.IPv6Any : IPAddress.Any;
            };

            // Warn the user if the set address is not one of the unspecified addresses
            if (!listenAddress.Equals(IPAddress.IPv6Any) && !listenAddress.Equals(IPAddress.Any))
                LunaLog.Warning("ListenAddress is not the unspecified address ([::] or 0.0.0.0). This is very unlikely to be correct and the server may not work.");
            
            // Ensure that the OS supports IPv6 if we're using it
            if (listenAddress.AddressFamily == AddressFamily.InterNetworkV6 && !Socket.OSSupportsIPv6)
            {
                LunaLog.Warning("The OS does not support IPv6 or it has been disabled, falling back on 0.0.0.0. " +
                "Consider enabling it for better reachability and connection success rate.");
                listenAddress = IPAddress.Any;
            }
            ServerContext.Config.LocalAddress = listenAddress;
            // Listen on dual-stack when we're using IPv6
            ServerContext.Config.DualStack = listenAddress.AddressFamily == AddressFamily.InterNetworkV6;

            ServerContext.Config.Port = ConnectionSettings.SettingsStore.Port;
            ServerContext.Config.AutoExpandMTU = ConnectionSettings.SettingsStore.AutoExpandMtu;
            ServerContext.Config.MaximumTransmissionUnit = ConnectionSettings.SettingsStore.MaximumTransmissionUnit;
            ServerContext.Config.MaximumConnections = GeneralSettings.SettingsStore.MaxPlayers;
            ServerContext.Config.PingInterval = (float)TimeSpan.FromMilliseconds(ConnectionSettings.SettingsStore.HearbeatMsInterval).TotalSeconds;
            ServerContext.Config.ConnectionTimeout = (float)TimeSpan.FromMilliseconds(ConnectionSettings.SettingsStore.ConnectionMsTimeout).TotalSeconds;

            if (LunaNetUtils.IsUdpPortInUse(ServerContext.Config.Port))
            {
                throw new HandledException($"Port {ServerContext.Config.Port} is already in use");
            }

            ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            if (LogSettings.SettingsStore.LogLevel >= LogLevels.NetworkDebug)
            {
                ServerContext.Config.EnableMessageType(NetIncomingMessageType.DebugMessage);
            }

            if (LogSettings.SettingsStore.LogLevel >= LogLevels.VerboseNetworkDebug)
            {
                ServerContext.Config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            }

#if DEBUG
            if (DebugSettings.SettingsStore?.SimulatedLossChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                ServerContext.Config.SimulatedLoss = DebugSettings.SettingsStore.SimulatedLossChance / 100f;
            }
            if (DebugSettings.SettingsStore?.SimulatedDuplicatesChance < 100 && DebugSettings.SettingsStore?.SimulatedLossChance > 0)
            {
                ServerContext.Config.SimulatedDuplicatesChance = DebugSettings.SettingsStore.SimulatedDuplicatesChance / 100f;
            }
            ServerContext.Config.SimulatedRandomLatency = (float)TimeSpan.FromMilliseconds((double)DebugSettings.SettingsStore?.MaxSimulatedRandomLatencyMs).TotalSeconds;
            ServerContext.Config.SimulatedMinimumLatency = (float)TimeSpan.FromMilliseconds((double)DebugSettings.SettingsStore?.MinSimulatedLatencyMs).TotalSeconds;
#endif

            ServerContext.Config.AutoFlushSendQueue = false;

            Server = new NetServer(ServerContext.Config);
            Server.Start();

            ServerContext.ServerStarting = false;
        }

        public static async Task StartReceivingMessagesAsync()
        {
            try
            {
                while (ServerContext.ServerRunning)
                {
                    var msg = Server.ReadMessage();
                    if (msg != null)
                    {
                        var client = TryGetClient(msg);
                        switch (msg.MessageType)
                        {
                            case NetIncomingMessageType.ConnectionApproval:
                                if (ServerContext.UsePassword)
                                {
                                    var password = msg.ReadString();
                                    if (password != GeneralSettings.SettingsStore.Password)
                                    {
                                        msg.SenderConnection.Deny("Invalid password");
                                        break;
                                    }
                                }
                                msg.SenderConnection.Approve();
                                break;
                            case NetIncomingMessageType.Data:
                                ClientMessageReceiver.ReceiveCallback(client, msg);
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                LunaLog.Warning(msg.ReadString());
                                break;
                            case NetIncomingMessageType.DebugMessage:
                                LunaLog.NetworkDebug(msg.ReadString());
                                break;
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.NetworkVerboseDebug(msg.ReadString());
                                break;
                            case NetIncomingMessageType.Error:
                                LunaLog.Error(msg.ReadString());
                                break;
                            case NetIncomingMessageType.StatusChanged:
                                switch ((NetConnectionStatus)msg.ReadByte())
                                {
                                    case NetConnectionStatus.Connected:
                                        var endpoint = msg.SenderConnection.RemoteEndPoint;
                                        LunaLog.Normal($"New client Connection from {endpoint.Address}:{endpoint.Port}");
                                        ClientConnectionHandler.ConnectClient(msg.SenderConnection);
                                        break;
                                    case NetConnectionStatus.Disconnected:
                                        var reason = msg.ReadString();
                                        if (client != null)
                                            ClientConnectionHandler.DisconnectClient(client, reason);
                                        break;
                                }
                                break;
                            case NetIncomingMessageType.UnconnectedData:
                                // Only process message if we are still waiting for STUN responses
                                if (await LidgrenMasterServer.ReceiveSTUNResponses.WaitAsync(0))
                                {
                                    var message = ServerContext.MasterServerMessageFactory.Deserialize(msg, LunaNetworkTime.UtcNow.Ticks);
                                    if (message.Data is MsSTUNSuccessResponseMsgData data)
                                    {
                                        LidgrenMasterServer.DetectedSTUNTransportAddresses.Add(data.TransportAddress);
                                    }
                                    LidgrenMasterServer.ReceiveSTUNResponses.Release();
                                }
                                break;
                            default:
                                var details = msg.PeekString();
                                LunaLog.Debug($"Lidgren: {msg.MessageType.ToString().ToUpper()} -- {details}");
                                break;
                        }
                    }
                    else
                    {
                        await Task.Delay(IntervalSettings.SettingsStore.SendReceiveThreadTickMs);
                    }
                }
            }
            catch (Exception e)
            {
                LunaLog.Fatal($"ERROR in thread receive! Details: {e}");
            }
        }

        private static ClientStructure TryGetClient(NetIncomingMessage msg)
        {
            if (msg.SenderConnection != null)
            {
                ServerContext.Clients.TryGetValue(msg.SenderConnection.RemoteEndPoint, out var client);
                return client;
            }
            return null;
        }

        public static void SendMessageToClient(ClientStructure client, IServerMessageBase message)
        {
            var outmsg = Server.CreateMessage(message.GetMessageSize());

            message.Data.SentTime = LunaNetworkTime.UtcNow.Ticks;
            message.Serialize(outmsg);

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += outmsg.LengthBytes;

            var sendResult = Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);
        }

        public static void FlushSendQueue()
        {
            Server.FlushSendQueue();
        }

        public static void ShutdownLidgrenServer()
        {
            Server.Shutdown("So long and thanks for all the fish");
        }
    }
}

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
using System.Threading;
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
            // Default to [::], fall back to 0.0.0.0 if IPv6 is not supported by OS
            if (!IPAddress.TryParse(ConnectionSettings.SettingsStore.ListenAddress, out var listenAddress))
            {
                LunaLog.Warning("Could not parse ListenAddress, falling back to 0.0.0.0");
                listenAddress = IPAddress.Any;
            };
            if (!listenAddress.Equals(IPAddress.IPv6Any) && !listenAddress.Equals(IPAddress.Any))
            {
                LunaLog.Warning("ListenAddress is not the unspecified address ([::] or 0.0.0.0). This is very unlikely to be correct, this server will not work.");
            }
            if (listenAddress.Equals(IPAddress.IPv6Any) && !Socket.OSSupportsIPv6)
            {
                LunaLog.Warning("OS does not support IPv6 or it has been disabled, changing ListenAddress to 0.0.0.0. " +
                "Consider enabling it for better reachability and connection success rate");
                listenAddress = IPAddress.Any;
            }
            ServerContext.Config.LocalAddress = listenAddress;
            // Listen on dual-stack for the unspecified address in IPv6 format ([::]).
            if (ServerContext.Config.LocalAddress.Equals(IPAddress.IPv6Any))
            {
                ServerContext.Config.DualStack = true;
            }

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

            Server = new NetServer(ServerContext.Config);
            Server.Start();

            ServerContext.ServerStarting = false;
        }

        public static void StartReceivingMessages()
        {
            _ = StartReceivingMessagesAsync();
        }

        public static async Task StartReceivingMessagesAsync()
        {
            // A single PeriodicTimer replaces the per-empty-poll `await Task.Delay(...)` that
            // previously dominated idle allocation in this thread. With no players connected the
            // loop runs ~200 times/sec and each Task.Delay was allocating a fresh Task plus an
            // async-state-machine box. PeriodicTimer allocates exactly once.
            //
            // Period is captured at start time and clamped to >=1 ms because PeriodicTimer rejects
            // a non-positive period; this matches the "Keep this value low but at least above 2ms"
            // guidance on SendReceiveThreadTickMs without crashing on misconfiguration.
            var tickMs = Math.Max(1, IntervalSettings.SettingsStore.SendReceiveThreadTickMs);
            using var idleTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(tickMs));
            var shutdownToken = MainServer.CancellationTokenSrc.Token;

            try
            {
                while (ServerContext.ServerRunning)
                {
                    var msg = Server.ReadMessage();
                    if (msg != null)
                    {
                        // Every NetIncomingMessage and its underlying byte[] payload comes from
                        // Lidgren's internal pool. The previous code dequeued messages here but
                        // never returned them, so the pool was forced to allocate fresh ones
                        // forever and the released messages piled up on the managed heap until
                        // GC reclaimed them. The finally block returns each message (and its
                        // storage buffer) to the pool exactly once, even if a handler throws.
                        try
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
                                    if (LidgrenMasterServer.ReceiveSTUNResponses.Wait(0))
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
                        catch (Exception e)
                        {
                            // A malformed packet must never terminate the receive thread.
                            // The most common culprit is UnconnectedData arriving during the
                            // STUN response window: random internet UDP traffic (port scans,
                            // stale NAT-punch packets, mismatched protocol versions) gets fed
                            // to MasterServerMessageFactory.Deserialize, which throws when the
                            // leading bytes do not map to a known MasterServerMessageType.
                            // Without this catch the exception escaped to the outer try/catch,
                            // logged a Fatal, and exited the while loop -- silently killing
                            // the entire server's networking. The regular client Data path is
                            // already protected inside MessageReceiver.DeserializeMessage; this
                            // is the safety net for everything else flowing through the switch.
                            var sender = msg.SenderEndPoint?.ToString() ?? "<unknown>";
                            LunaLog.Error($"Dropping incoming {msg.MessageType} message from {sender}: {e}");
                        }
                        finally
                        {
                            Server.Recycle(msg);
                        }
                    }
                    else
                    {
                        // WaitForNextTickAsync returns false when the timer is disposed or the
                        // token is cancelled. Either signal means "stop looping" so we just exit.
                        if (!await idleTimer.WaitForNextTickAsync(shutdownToken))
                            break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown path — the cancellation token was tripped while we were waiting
                // for the next tick. Nothing to log; the rest of the shutdown sequence handles it.
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

            //Force send of packets
            FlushSendQueue();
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

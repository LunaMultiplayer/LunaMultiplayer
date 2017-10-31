using Lidgren.Network;
using LunaCommon;
using LunaCommon.Enums;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaServer.Client;
using LunaServer.Context;
using LunaServer.Log;
using LunaServer.Server;
using LunaServer.Settings;
using LunaServer.System;
using LunaServer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace LunaServer.Lidgren
{
    public class LidgrenServer
    {
        private static List<IPEndPoint> MasterServerEndpoints { get; } = new List<IPEndPoint>();
        private static NetServer Server { get; set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        private int MasterServerRegistrationMsInterval
            => GeneralSettings.SettingsStore.MasterServerRegistrationMsInterval;

        public void SetupLidgrenServer()
        {
            if (PortIsInUse(ServerContext.Config.Port))
            {
                throw new HandledException($"Port {ServerContext.Config.Port} is already in use");
            }

            ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

            Server = new NetServer(ServerContext.Config);
            Server.Start();
            ServerContext.ServerStarting = false;
        }

        private static bool PortIsInUse(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var udpConnInfoArray = ipGlobalProperties.GetActiveUdpListeners();

            return udpConnInfoArray.Any(tcpi => tcpi.Port == port);
        }

        public void StartReceiveingMessages()
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
                                msg.SenderConnection.Approve();
                                break;
                            case NetIncomingMessageType.Data:
                                ClientMessageReceiver.ReceiveCallback(client, msg);
                                break;
                            case NetIncomingMessageType.WarningMessage:
                                LunaLog.Debug($"Lidgren WARNING: {msg.ReadString()}");
                                break;
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.Debug($"Lidgren DEBUG: {msg.MessageType}-- {msg.PeekString()}");
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
                            default:
                                var details = msg.PeekString();
                                LunaLog.Debug($"Lidgren: {msg.MessageType.ToString().ToUpper()} -- {details}");
                                break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
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

        public void SendMessageToClient(ClientStructure client, IServerMessageBase message)
        {
            if (message.MessageType == ServerMessageType.SyncTime)
                SyncTimeSystem.RewriteMessage(client, message);

            message.Data.SentTime = DateTime.UtcNow.Ticks;
            var messageBytes = message.Serialize(GeneralSettings.SettingsStore.CompressionEnabled);

            if (messageBytes == null)
            {
                LunaLog.Error("Error serializing message!");
                return;
            }

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += messageBytes.Length;

            var outmsg = Server.CreateMessage(messageBytes.Length);
            outmsg.Write(messageBytes);

            Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);
            Server.FlushSendQueue(); //Manually force to send the msg
        }

        public void ShutdownLidgrenServer()
        {
            Server.Shutdown("Goodbye and thanks for all the fish");
        }

        public void RegisterWithMasterServer()
        {
            if (!GeneralSettings.SettingsStore.RegisterWithMasterServer) return;

            LunaLog.Normal("Registering with master servers...");

            var adr = NetUtility.GetMyAddress(out var _);
            var endpoint = new IPEndPoint(adr, ServerContext.Config.Port);

            if (!MasterServerEndpoints.Any())
                MasterServerEndpoints.AddRange(MasterServerRetriever.RetrieveWorkingMasterServersEndpoints()
                    .Select(Common.CreateEndpointFromString));

            while (ServerContext.ServerRunning)
            {
                var msgData = new MsRegisterServerMsgData
                {
                    Id = Server.UniqueIdentifier,
                    Cheats = GeneralSettings.SettingsStore.Cheats,
                    ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast,
                    Description = GeneralSettings.SettingsStore.Description,
                    DropControlOnExit = GeneralSettings.SettingsStore.Cheats,
                    DropControlOnExitFlight = GeneralSettings.SettingsStore.Cheats,
                    DropControlOnVesselSwitching = GeneralSettings.SettingsStore.Cheats,
                    GameMode = (int)GeneralSettings.SettingsStore.GameMode,
                    InternalEndpoint = $"{endpoint.Address}:{endpoint.Port}",
                    MaxPlayers = GeneralSettings.SettingsStore.MaxPlayers,
                    ModControl = (int)GeneralSettings.SettingsStore.ModControl,
                    PlayerCount = ServerContext.Clients.Count,
                    ServerName = GeneralSettings.SettingsStore.ServerName,
                    ServerVersion = VersionInfo.FullVersionNumber,
                    VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval,
                    SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval,
                    WarpMode = (int)GeneralSettings.SettingsStore.WarpMode
                };

                msgData.Description = msgData.Description.Length > 200
                            ? msgData.Description.Substring(0, 200)
                            : msgData.Description;

                msgData.ServerName = msgData.ServerName.Length > 30
                    ? msgData.ServerName.Substring(0, 30)
                    : msgData.ServerName;

                var msg = ServerContext.MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                var msgBytes = ServerContext.MasterServerMessageFactory.Serialize(msg);

                foreach (var masterServer in MasterServerEndpoints)
                {
                    try
                    {
                        var outMsg = Server.CreateMessage(msgBytes.Length);
                        outMsg.Write(msgBytes);
                        Server.SendUnconnectedMessage(outMsg, masterServer);
                        Server.FlushSendQueue();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }

                Thread.Sleep(MasterServerRegistrationMsInterval);
            }
        }
    }
}
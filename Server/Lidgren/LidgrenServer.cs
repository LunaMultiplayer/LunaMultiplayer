using Lidgren.Network;
using LunaCommon;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaCommon.Time;
using Server.Client;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;
using Server.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Server.Lidgren
{
    public class LidgrenServer
    {
        private static List<IPEndPoint> MasterServerEndpoints { get; } = new List<IPEndPoint>();
        private static NetServer Server { get; set; }
        public static MessageReceiver ClientMessageReceiver { get; set; } = new MessageReceiver();

        private static int MasterServerRegistrationMsInterval => GeneralSettings.SettingsStore.MasterServerRegistrationMsInterval < 5000 ? 
            5000 : GeneralSettings.SettingsStore.MasterServerRegistrationMsInterval;

        public void SetupLidgrenServer()
        {
            if (Common.PortIsInUse(ServerContext.Config.Port))
            {
                throw new HandledException($"Port {ServerContext.Config.Port} is already in use");
            }

            ServerContext.Config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            ServerContext.Config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);

            Server = new NetServer(ServerContext.Config);
            Server.Start();
            ServerContext.ServerStarting = false;
        }

        public async void StartReceiveingMessages()
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
                                LunaLog.Warning(msg.ReadString());
                                break;
                            case NetIncomingMessageType.ConnectionLatencyUpdated:
                            case NetIncomingMessageType.DebugMessage:
                            case NetIncomingMessageType.VerboseDebugMessage:
                                LunaLog.NetworkDebug(msg.ReadString());
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
                            default:
                                var details = msg.PeekString();
                                LunaLog.Debug($"Lidgren: {msg.MessageType.ToString().ToUpper()} -- {details}");
                                break;
                        }
                        Server.Recycle(msg);
                    }
                    else
                    {
                        await Task.Delay(GeneralSettings.SettingsStore.SendReceiveThreadTickMs);
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
            message.Data.SentTime = LunaTime.UtcNow.Ticks;
            var messageBytes = message.Serialize(GeneralSettings.SettingsStore.CompressionEnabled);
            if (messageBytes == null)
            {
                LunaLog.Error("Error serializing message!");
                return;
            }

            client.LastSendTime = ServerContext.ServerClock.ElapsedMilliseconds;
            client.BytesSent += messageBytes.Length;

            //Lidgren already recycle messages by itself
            var outmsg = Server.CreateMessage(messageBytes.Length);
            outmsg.Write(messageBytes);

            Server.SendMessage(outmsg, client.Connection, message.NetDeliveryMethod, message.Channel);
            Server.FlushSendQueue(); //Manually force to send the msg
        }

        public void ShutdownLidgrenServer()
        {
            Server.Shutdown("Goodbye and thanks for all the fish");
        }

        public async void RegisterWithMasterServer()
        {
            if (!GeneralSettings.SettingsStore.RegisterWithMasterServer) return;

            LunaLog.Normal("Registering with master servers...");

            var adr = LunaNetUtils.GetMyAddress(out var _);
            if (adr == null) return;

            var endpoint = new IPEndPoint(adr, ServerContext.Config.Port);

            if (!MasterServerEndpoints.Any())
                MasterServerEndpoints.AddRange(MasterServerRetriever.RetrieveWorkingMasterServersEndpoints()
                    .Select(Common.CreateEndpointFromString));

            while (ServerContext.ServerRunning)
            {
                var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<MsRegisterServerMsgData>();
                msgData.Id = Server.UniqueIdentifier;
                msgData.Cheats = GeneralSettings.SettingsStore.Cheats;
                msgData.ShowVesselsInThePast = GeneralSettings.SettingsStore.ShowVesselsInThePast;
                msgData.Description = GeneralSettings.SettingsStore.Description;
                msgData.DropControlOnExit = GeneralSettings.SettingsStore.Cheats;
                msgData.DropControlOnExitFlight = GeneralSettings.SettingsStore.Cheats;
                msgData.DropControlOnVesselSwitching = GeneralSettings.SettingsStore.Cheats;
                msgData.GameMode = (int)GeneralSettings.SettingsStore.GameMode;
                msgData.InternalEndpoint = $"{endpoint.Address}:{endpoint.Port}";
                msgData.MaxPlayers = GeneralSettings.SettingsStore.MaxPlayers;
                msgData.ModControl = (int)GeneralSettings.SettingsStore.ModControl;
                msgData.PlayerCount = ServerContext.Clients.Count;
                msgData.ServerName = GeneralSettings.SettingsStore.ServerName;
                msgData.ServerVersion = LmpVersioning.CurrentVersion;
                msgData.VesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.VesselUpdatesSendMsInterval;
                msgData.SecondaryVesselUpdatesSendMsInterval = GeneralSettings.SettingsStore.SecondaryVesselUpdatesSendMsInterval;
                msgData.WarpMode = (int)GeneralSettings.SettingsStore.WarpMode;
                msgData.TerrainQuality = (int)GeneralSettings.SettingsStore.TerrainQuality;

                msgData.Description = msgData.Description.Length > 200
                            ? msgData.Description.Substring(0, 200)
                            : msgData.Description;

                msgData.ServerName = msgData.ServerName.Length > 30
                    ? msgData.ServerName.Substring(0, 30)
                    : msgData.ServerName;

                var msg = ServerContext.MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
                var msgBytes = msg.Serialize(true);

                foreach (var masterServer in MasterServerEndpoints)
                {
                    RegisterWithMasterServer(msgBytes, masterServer);
                }

                await Task.Delay(MasterServerRegistrationMsInterval);
            }
        }

        private static void RegisterWithMasterServer(byte[] msgBytes, IPEndPoint masterServer)
        {
            Task.Run(() =>
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
            });
        }
    }
}